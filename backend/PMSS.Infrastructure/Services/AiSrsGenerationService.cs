using System.ClientModel;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Infrastructure.Configuration;

namespace PMSS.Infrastructure.Services;

public class AiSrsGenerationService : IAiSrsGenerationService
{
    private readonly IJiraApiService _jiraApiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GitHubModelsSettings _modelsSettings;
    private readonly ILogger<AiSrsGenerationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiSrsGenerationService(
        IJiraApiService jiraApiService,
        IUnitOfWork unitOfWork,
        IOptions<GitHubModelsSettings> modelsSettings,
        ILogger<AiSrsGenerationService> logger)
    {
        _jiraApiService = jiraApiService;
        _unitOfWork = unitOfWork;
        _modelsSettings = modelsSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> GenerateSrsMarkdownAsync(Guid projectId)
    {
        try
        {
            var srsContent = await GenerateSrsContentAsync(projectId);
            if (srsContent.ErrorMessage != null)
                return ApiResponse<string>.ErrorResponse(srsContent.ErrorMessage);

            return ApiResponse<string>.SuccessResponse(srsContent.Content!, "AI-generated SRS markdown created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI SRS markdown for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse("Error generating AI SRS document", ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateSrsDocxAsync(Guid projectId)
    {
        try
        {
            var srsContent = await GenerateSrsContentAsync(projectId);
            if (srsContent.ErrorMessage != null)
                return ApiResponse<byte[]>.ErrorResponse(srsContent.ErrorMessage);

            var docxBytes = BuildDocx(srsContent.ProjectName!, srsContent.Content!);

            return ApiResponse<byte[]>.SuccessResponse(docxBytes, "AI-generated SRS document created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI SRS for project {ProjectId}", projectId);
            return ApiResponse<byte[]>.ErrorResponse("Error generating AI SRS document", ex.Message);
        }
    }

    private async Task<(string? Content, string? ProjectName, string? ErrorMessage)> GenerateSrsContentAsync(Guid projectId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
            return (null, null, "Project not found");

        var jiraConfig = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);
        if (jiraConfig == null || !jiraConfig.IsActive)
            return (null, null, "No active Jira configuration found for this project");

        string rawJson;
        try
        {
            rawJson = await _jiraApiService.FetchRawJiraIssuesAsync(projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Jira issues for project {ProjectId}", projectId);
            return (null, null, $"Failed to fetch Jira issues: {ex.Message}");
        }

        var issuesSummary = ExtractIssuesSummary(rawJson);

        var projectClass = project.Class;
        var course = projectClass?.Course;
        var courseName = course?.Name ?? "N/A";
        var classCode = projectClass?.ClassCode ?? "N/A";
        var className = projectClass != null ? $"{course?.Code}-{classCode}" : "N/A";

        var members = await _unitOfWork.ProjectMembers.GetMembersByProjectIdAsync(projectId);
        var memberNames = members.Select(m => m.User.Name).ToList();

        var srsContent = await GenerateSrsWithAIAsync(
            project.Name,
            project.Description ?? string.Empty,
            jiraConfig.ProjectKey,
            courseName,
            className,
            classCode,
            memberNames,
            issuesSummary);

        return (srsContent, project.Name, null);
    }

    private static string ExtractIssuesSummary(string rawJson)
    {
        var root = JsonSerializer.Deserialize<JsonElement>(rawJson, JsonOptions);
        if (!root.TryGetProperty("issues", out var issuesArray))
            return "No issues found.";

        var sb = new StringBuilder();
        foreach (var issue in issuesArray.EnumerateArray())
        {
            var key = issue.GetProperty("key").GetString() ?? "";
            var fields = issue.GetProperty("fields");
            var summary = GetString(fields, "summary");
            var status = GetNestedString(fields, "status", "name");
            var type = GetNestedString(fields, "issuetype", "name");
            var priority = GetNestedString(fields, "priority", "name");
            var description = GetAtlassianDocText(fields, "description");
            var assignee = GetNestedString(fields, "assignee", "displayName");
            var labels = GetStringArray(fields, "labels");
            var components = GetNamedArray(fields, "components");
            var parentKey = GetNestedString(fields, "parent", "key");

            sb.AppendLine($"- [{key}] ({type}, {priority}, {status}) {summary}");
            if (!string.IsNullOrWhiteSpace(description))
                sb.AppendLine($"  Description: {Truncate(description, 300)}");
            if (!string.IsNullOrWhiteSpace(assignee))
                sb.AppendLine($"  Assignee: {assignee}");
            if (labels.Count > 0)
                sb.AppendLine($"  Labels: {string.Join(", ", labels)}");
            if (components.Count > 0)
                sb.AppendLine($"  Components: {string.Join(", ", components)}");
            if (!string.IsNullOrWhiteSpace(parentKey))
                sb.AppendLine($"  Parent: {parentKey}");
        }

        return sb.ToString();
    }

    private async Task<string> GenerateSrsWithAIAsync(
        string projectName,
        string projectDescription,
        string jiraProjectKey,
        string courseName,
        string className,
        string classCode,
        List<string> memberNames,
        string issuesSummary)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(_modelsSettings.GitHubToken),
            new OpenAIClientOptions { Endpoint = new Uri(_modelsSettings.Endpoint) });

        var chatClient = client.GetChatClient(_modelsSettings.ModelName);

        var systemPrompt = """
            You are a professional software requirements analyst. Generate a complete
            Software Requirements Specification (SRS) document in **Markdown** format
            based on the provided Jira project data.

            You MUST follow EXACTLY this structure and formatting:

            # [Project Name]
            # Software Requirement Specification

            **Class Code:** [ClassCode]
            **Group Code:** [GroupCode]
            **Generated:** [Date]

            ---

            # Record of Change
            *A - Added | M - Modified | D - Deleted*
            | Effective Date | Changed Items | A / M / D | Change Description | New Version |
            (fill with Initial row version 1.0)

            ---

            # SIGNATURE PAGE
            ## ORIGINATOR
            | Name | Date | Role/Title |
            (fill with the provided team members)

            ## REVIEWERS
            | Name | Date | Role |
            (leave as placeholder)

            ---

            # 1. Introduction
            ## 1.1 Purpose
            ## 1.2 Definitions, Acronyms
            ## 1.3 References

            # 2. Overall Description
            ## 2.1 Product Perspective
            ## 2.2 Business Process
            ## 2.3 User Classes

            # 3. FUNCTIONAL REQUIREMENTS
            ## 3.1 Use Case Diagram
            (describe what the overall use case diagram should contain)
            ## 3.2 Use Case Specifications
            For EACH major feature/epic from Jira, create a Use Case Specification table:
            | Field | Value |
            | Use-case No. | UC-X |
            | Use-case Name | ... |
            | Priority | ... |
            | Primary Actor | ... |
            | Secondary Actor | ... |
            Then: Description, Triggers, Preconditions (PRE-X), Post Conditions (POST-X),
            Main Success Scenario (numbered steps), Alternative Scenario, Exceptions,
            Relationships, Business Rules (BR-XX references).

            ## 3.3 State Diagrams
            (describe relevant state diagrams based on Jira data)
            ## 3.4 Data Flow Diagrams
            (describe relevant DFDs)
            ## 3.5 Logical Data Model
            (describe ERD/schema based on features)

            # 4. NON-FUNCTIONAL REQUIREMENTS
            ## 4.1 Usability
            ## 4.2 Reliability
            ## 4.3 Performance
            ## 4.4 Reusability
            ## 4.5 Scalability

            # 5. Supporting Information
            ## 5.1 Appendices
            ## Appendix A — Business Rules Reference
            (BR-XX format, grouped by category)
            ## Appendix B — Integration Requirements
            ## Appendix C — Security Requirements

            RULES:
            - Output valid Markdown only.
            - Use "# ", "## ", "### " for heading levels.
            - Use "- " for bullet points and "| " delimited rows for tables.
            - Derive REAL requirements, use cases, and business rules from the Jira issues.
            - Map each use case back to Jira issue keys where applicable.
            - Be thorough, professional, and specific.
            """;

        var membersInfo = memberNames.Count > 0
            ? string.Join(", ", memberNames)
            : "No members listed";

        var userPrompt = $"""
            Project Name: {projectName}
            Jira Project Key: {jiraProjectKey}
            Course: {courseName}
            Class Code: {classCode}
            Class: {className}
            Group Code: {jiraProjectKey}
            Team Members: {membersInfo}
            Project Description: {(string.IsNullOrWhiteSpace(projectDescription) ? "Not provided" : projectDescription)}

            Jira Issues:
            {issuesSummary}

            Generate the full SRS document in Markdown now.
            """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 8192,
            Temperature = 0.3f
        };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        return completion.Value.Content[0].Text;
    }

    private static byte[] BuildDocx(string projectName, string srsContent)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AddStyleDefinitions(mainPart);

            // Title
            AddParagraph(body, $"Software Requirements Specification", "Title");
            AddParagraph(body, projectName, "Subtitle");
            AddParagraph(body, $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", "Normal");
            AddParagraph(body, "", "Normal");

            // Parse AI content into the document
            var lines = srsContent.Split('\n');
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddParagraph(body, "", "Normal");
                    continue;
                }

                if (line.StartsWith("### "))
                {
                    AddParagraph(body, line[4..], "Heading3");
                }
                else if (line.StartsWith("## "))
                {
                    AddParagraph(body, line[3..], "Heading2");
                }
                else if (line.StartsWith("# "))
                {
                    AddParagraph(body, line[2..], "Heading1");
                }
                else if (line.TrimStart().StartsWith("| "))
                {
                    // Table rows — collect contiguous table lines and build table
                    // For simplicity, render as a styled paragraph
                    AddParagraph(body, line.Trim(), "Normal");
                }
                else if (line.TrimStart().StartsWith("- "))
                {
                    AddBulletParagraph(body, line.TrimStart()[2..]);
                }
                else
                {
                    AddParagraph(body, line, "Normal");
                }
            }
        }

        return stream.ToArray();
    }

    private static void AddStyleDefinitions(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        styles.Append(CreateStyle("Title", "Title", 28, true));
        styles.Append(CreateStyle("Subtitle", "Subtitle", 22, false, italic: true));
        styles.Append(CreateStyle("Heading1", "heading 1", 24, true));
        styles.Append(CreateStyle("Heading2", "heading 2", 20, true));
        styles.Append(CreateStyle("Heading3", "heading 3", 16, true));
        styles.Append(CreateStyle("Normal", "Normal", 11, false));
        styles.Append(CreateStyle("ListBullet", "List Bullet", 11, false));

        stylesPart.Styles = styles;
    }

    private static Style CreateStyle(string styleId, string styleName, int fontSize, bool bold, bool italic = false)
    {
        var style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = styleId
        };
        style.Append(new StyleName { Val = styleName });

        var rPr = new StyleRunProperties();
        rPr.Append(new FontSize { Val = (fontSize * 2).ToString() });
        rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        if (bold)
            rPr.Append(new Bold());
        if (italic)
            rPr.Append(new Italic());

        style.Append(rPr);
        return style;
    }

    private static void AddParagraph(Body body, string text, string styleId)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new ParagraphStyleId { Val = styleId });
        para.Append(pPr);

        if (!string.IsNullOrEmpty(text))
        {
            var run = new Run();
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
        }

        body.Append(para);
    }

    private static void AddBulletParagraph(Body body, string text)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new ParagraphStyleId { Val = "ListBullet" });
        para.Append(pPr);

        var run = new Run();
        run.Append(new Text($"• {text}") { Space = SpaceProcessingModeValues.Preserve });
        para.Append(run);

        body.Append(para);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    #region JSON Helpers

    private static string GetString(JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String
            ? val.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string GetNestedString(JsonElement el, string prop, string nested)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Object)
            return GetString(val, nested);
        return string.Empty;
    }

    private static List<string> GetStringArray(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Array)
            return val.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.String)
                .Select(v => v.GetString()!)
                .ToList();
        return [];
    }

    private static List<string> GetNamedArray(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Array)
            return val.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.Object)
                .Select(v => GetString(v, "name"))
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();
        return [];
    }

    private static string GetAtlassianDocText(JsonElement fields, string prop)
    {
        if (!fields.TryGetProperty(prop, out var desc) || desc.ValueKind != JsonValueKind.Object)
            return string.Empty;

        if (!desc.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var parts = new List<string>();
        ExtractTextFromAdf(content, parts);
        return string.Join(" ", parts);
    }

    private static void ExtractTextFromAdf(JsonElement element, List<string> parts)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                ExtractTextFromAdf(item, parts);
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("type", out var type) && type.GetString() == "text"
                && element.TryGetProperty("text", out var text))
            {
                parts.Add(text.GetString() ?? string.Empty);
            }

            if (element.TryGetProperty("content", out var nested))
                ExtractTextFromAdf(nested, parts);
        }
    }

    #endregion
}
