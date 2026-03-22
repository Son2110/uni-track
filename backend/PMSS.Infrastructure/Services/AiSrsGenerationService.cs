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
    private readonly IGithubApiService _githubApiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GitHubModelsSettings _modelsSettings;
    private readonly OpenAISettings _openAISettings;
    private readonly ILogger<AiSrsGenerationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiSrsGenerationService(
        IJiraApiService jiraApiService,
        IGithubApiService githubApiService,
        IUnitOfWork unitOfWork,
        IOptions<GitHubModelsSettings> modelsSettings,
        IOptions<OpenAISettings> openAISettings,
        ILogger<AiSrsGenerationService> logger)
    {
        _jiraApiService = jiraApiService;
        _githubApiService = githubApiService;
        _unitOfWork = unitOfWork;
        _modelsSettings = modelsSettings.Value;
        _openAISettings = openAISettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> GenerateSrsMarkdownAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null)
    {
        try
        {
            var srsContent = await GenerateSrsContentAsync(projectId, usePaidModel, modelOption);
            if (srsContent.ErrorMessage != null)
                return ApiResponse<string>.ErrorResponse(srsContent.ErrorMessage);

            return ApiResponse<string>.SuccessResponse(srsContent.Content!, "AI-generated SRS markdown created successfully");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "AI API error generating SRS markdown for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse(GetFriendlyAIErrorMessage(ex, usePaidModel));
        }
        catch (Exception ex) when (IsTimeoutException(ex))
        {
            _logger.LogError(ex, "AI request timed out for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse(
                $"The AI model took too long to respond (timeout: {_openAISettings.NetworkTimeoutInSeconds}s). " +
                "The project may have too many Jira issues. Try again or increase NetworkTimeoutInSeconds in the OpenAI configuration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI SRS markdown for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse("Error generating AI SRS document", ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateSrsDocxAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null)
    {
        try
        {
            var srsContent = await GenerateSrsContentAsync(projectId, usePaidModel, modelOption);
            if (srsContent.ErrorMessage != null)
                return ApiResponse<byte[]>.ErrorResponse(srsContent.ErrorMessage);

            var docxBytes = BuildDocx(srsContent.ProjectName!, srsContent.Content!);

            return ApiResponse<byte[]>.SuccessResponse(docxBytes, "AI-generated SRS document created successfully");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "AI API error generating SRS docx for project {ProjectId}", projectId);
            return ApiResponse<byte[]>.ErrorResponse(GetFriendlyAIErrorMessage(ex, usePaidModel));
        }
        catch (Exception ex) when (IsTimeoutException(ex))
        {
            _logger.LogError(ex, "AI request timed out for project {ProjectId}", projectId);
            return ApiResponse<byte[]>.ErrorResponse(
                $"The AI model took too long to respond (timeout: {_openAISettings.NetworkTimeoutInSeconds}s). " +
                "The project may have too many Jira issues. Try again or increase NetworkTimeoutInSeconds in the OpenAI configuration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI SRS for project {ProjectId}", projectId);
            return ApiResponse<byte[]>.ErrorResponse("Error generating AI SRS document", ex.Message);
        }
    }

    public async Task<ApiResponse<string>> GenerateGithubReportMarkdownAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null)
    {
        try
        {
            var reportContent = await GenerateGithubReportContentAsync(projectId, usePaidModel, modelOption);
            if (reportContent.ErrorMessage != null)
                return ApiResponse<string>.ErrorResponse(reportContent.ErrorMessage);

            return ApiResponse<string>.SuccessResponse(reportContent.Content!, "AI-generated GitHub report markdown created successfully");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "AI API error generating GitHub report markdown for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse(GetFriendlyAIErrorMessage(ex, usePaidModel));
        }
        catch (Exception ex) when (IsTimeoutException(ex))
        {
            _logger.LogError(ex, "AI request timed out for GitHub report of project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse(
                $"The AI model took too long to respond (timeout: {_openAISettings.NetworkTimeoutInSeconds}s). " +
                "The project may have too much GitHub activity data. Try again or increase NetworkTimeoutInSeconds in the OpenAI configuration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI GitHub report markdown for project {ProjectId}", projectId);
            return ApiResponse<string>.ErrorResponse("Error generating AI GitHub report", ex.Message);
        }
    }

    private async Task<(string? Content, string? ProjectName, string? ErrorMessage)> GenerateSrsContentAsync(Guid projectId, bool usePaidModel, string? modelOption)
    {
        if (usePaidModel)
        {
            if (string.IsNullOrWhiteSpace(_openAISettings.ApiKey))
                return (null, null, "OpenAI API key is not configured. Please set the ApiKey in the OpenAI configuration to use the paid model.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_modelsSettings.GitHubToken))
                return (null, null, "GitHub Models API token is not configured. Please set the GitHubToken in the GitHubModels configuration.");
        }

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
        var semester = projectClass?.Semester;
        var courseName = course?.Name ?? "N/A";
        var classCode = projectClass?.ClassCode ?? "N/A";
        var className = projectClass != null ? $"{course?.Code}-{classCode}" : "N/A";
        var semesterName = semester?.Name ?? "N/A";

        var members = await _unitOfWork.ProjectMembers.GetMembersByProjectIdAsync(projectId);
        var memberNames = members.Select(m => m.User.Name).ToList();

        var srsContent = usePaidModel
            ? await GenerateSrsWithPaidModelAsync(
                project.Name,
                project.Description ?? string.Empty,
                jiraConfig.ProjectKey,
                courseName,
                className,
                classCode,
                semesterName,
                memberNames,
                issuesSummary,
                modelOption)
            : await GenerateSrsWithAIAsync(
                project.Name,
                project.Description ?? string.Empty,
                jiraConfig.ProjectKey,
                courseName,
                className,
                classCode,
                semesterName,
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
            var parentSummary = GetNestedString(fields, "parent", "fields", "summary");
            var created = GetString(fields, "created");
            var updated = GetString(fields, "updated");
            var fixVersions = GetNamedArray(fields, "fixVersions");
            var resolution = GetNestedString(fields, "resolution", "name");
            var environment = GetAtlassianDocText(fields, "environment");
            var subtasks = ExtractSubtasks(fields);
            var issueLinks = ExtractIssueLinks(fields);
            var comments = ExtractComments(fields);

            sb.AppendLine($"- [{key}] ({type}, {priority}, {status}) {summary}");
            if (!string.IsNullOrWhiteSpace(description))
                sb.AppendLine($"  Description: {description}");
            if (!string.IsNullOrWhiteSpace(assignee))
                sb.AppendLine($"  Assignee: {assignee}");
            if (!string.IsNullOrWhiteSpace(resolution))
                sb.AppendLine($"  Resolution: {resolution}");
            if (labels.Count > 0)
                sb.AppendLine($"  Labels: {string.Join(", ", labels)}");
            if (components.Count > 0)
                sb.AppendLine($"  Components: {string.Join(", ", components)}");
            if (!string.IsNullOrWhiteSpace(parentKey))
                sb.AppendLine($"  Parent: {parentKey}{(string.IsNullOrWhiteSpace(parentSummary) ? "" : $" ({parentSummary})")}");
            if (subtasks.Count > 0)
                sb.AppendLine($"  Subtasks: {string.Join("; ", subtasks)}");
            if (!string.IsNullOrWhiteSpace(created))
                sb.AppendLine($"  Created: {created}");
            if (!string.IsNullOrWhiteSpace(updated))
                sb.AppendLine($"  Updated: {updated}");
            if (fixVersions.Count > 0)
                sb.AppendLine($"  Fix Versions: {string.Join(", ", fixVersions)}");
            if (!string.IsNullOrWhiteSpace(environment))
                sb.AppendLine($"  Environment: {environment}");
            if (issueLinks.Count > 0)
                sb.AppendLine($"  Links: {string.Join("; ", issueLinks)}");
            if (comments.Count > 0)
            {
                sb.AppendLine($"  Comments:");
                foreach (var comment in comments)
                    sb.AppendLine($"    - {comment}");
            }
        }

        return sb.ToString();
    }

    private const int MaxInputTokens = 8000;
    private const int TokensPerChar = 4;
    private const int PromptOverheadTokens = 500;

    private async Task<string> GenerateSrsWithAIAsync(
        string projectName,
        string projectDescription,
        string jiraProjectKey,
        string courseName,
        string className,
        string classCode,
        string semesterName,
        List<string> memberNames,
        string issuesSummary)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(_modelsSettings.GitHubToken),
            new OpenAIClientOptions { Endpoint = new Uri(_modelsSettings.Endpoint) });

        var chatClient = client.GetChatClient(_modelsSettings.ModelName);

        var todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var systemPrompt = $"""
            You are a software requirements analyst. Generate a complete SRS in Markdown based STRICTLY on provided Jira data only.
            Date: {todayDate}. Use this date for Generated, Record of Change, Signature Page dates.
            RULES: Only use info from Jira issues. Do NOT invent requirements, actors, or rules not in Jira. If a section has no Jira data, write "No data from Jira." Trace every use case/requirement to Jira keys. Use full output capacity.
            STRUCTURE:
            # [Project Name] / # Software Requirement Specification
            **Class Code/Group Code/Generated** → # Record of Change (Initial v1.0, {todayDate}) → # Signature Page (Originator=team members, Reviewers=placeholder)
            # 1. Introduction (1.1 Purpose, 1.2 Definitions, 1.3 References)
            # 2. Overall Description (2.1 Product Perspective, 2.2 Business Process, 2.3 User Classes)
            # 3. Functional Requirements: 3.1 Use Case Diagram, 3.2 Use Case Specs (for EACH Jira epic/story: UC table with No/Name/Jira Ref/Priority/Actors, then Description/Triggers/Pre-Post Conditions/Main+Alt Scenarios/Exceptions/Business Rules BR-XX), 3.3 State Diagrams, 3.4 DFDs, 3.5 Logical Data Model
            # 4. Non-Functional Requirements (4.1-4.5: Usability/Reliability/Performance/Reusability/Scalability — only from Jira data)
            # 5. Supporting Info: Appendix A=Business Rules BR-XX, B=Integration, C=Security
            """;

        var membersInfo = memberNames.Count > 0
            ? string.Join(", ", memberNames)
            : "No members listed";

        var userPromptPrefix = $"""
            Project: {projectName} | Key: {jiraProjectKey} | Course: {courseName} | Class: {className} | ClassCode: {classCode} | Semester: {semesterName} | Group: {jiraProjectKey}
            Members: {membersInfo}
            Description: {(string.IsNullOrWhiteSpace(projectDescription) ? "N/A" : projectDescription)}
            Date: {todayDate}
            === JIRA ISSUES (ONLY source of truth) ===
            """;

        const string userPromptSuffix = """
            === END JIRA ISSUES ===
            Generate the full SRS in Markdown. Use ONLY the Jira issues above. Do NOT fabricate any info not in the data.
            """;

        var trimmedIssues = TrimIssuesToFit(systemPrompt, userPromptPrefix, userPromptSuffix, issuesSummary);

        var userPrompt = $"{userPromptPrefix}\n{trimmedIssues}\n{userPromptSuffix}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 4096,
            Temperature = 0.2f
        };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        var responseText = ExtractCompletionText(completion.Value);
        return responseText;
    }

    private static string TrimIssuesToFit(string systemPrompt, string userPromptPrefix, string userPromptSuffix, string issuesSummary)
    {
        var fixedChars = systemPrompt.Length + userPromptPrefix.Length + userPromptSuffix.Length;
        var fixedTokens = (fixedChars / TokensPerChar) + PromptOverheadTokens;
        var availableTokensForIssues = MaxInputTokens - fixedTokens;

        if (availableTokensForIssues < 100)
            availableTokensForIssues = 100;

        var maxIssueChars = availableTokensForIssues * TokensPerChar;

        if (issuesSummary.Length <= maxIssueChars)
            return issuesSummary;

        var truncated = issuesSummary[..maxIssueChars];
        var lastNewline = truncated.LastIndexOf('\n');
        if (lastNewline > maxIssueChars / 2)
            truncated = truncated[..lastNewline];

        return truncated + "\n[... additional issues truncated to fit token limit ...]";
    }

    private async Task<string> GenerateSrsWithPaidModelAsync(
        string projectName,
        string projectDescription,
        string jiraProjectKey,
        string courseName,
        string className,
        string classCode,
        string semesterName,
        List<string> memberNames,
        string issuesSummary,
        string? modelOption)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(_openAISettings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(_openAISettings.Endpoint),
                NetworkTimeout = TimeSpan.FromSeconds(_openAISettings.NetworkTimeoutInSeconds)
            });

        var selectedModel = ResolvePaidModelName(_openAISettings.DefaultSrsModelOption, modelOption);
        var chatClient = client.GetChatClient(selectedModel);

        var todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var systemPrompt = $"""
            You are a professional software requirements analyst. Generate a complete and thorough
            Software Requirements Specification (SRS) document in **Markdown** format
            based STRICTLY and ONLY on the provided Jira project data.

            TODAY'S DATE: {todayDate}
            Use this date wherever a date is needed (Generated date, Record of Change, Signature Page, etc.).

            CRITICAL RULES:
            - ONLY use information explicitly present in the provided Jira issues. Do NOT invent, assume, or fabricate any requirements, features, actors, business rules, or details that are not directly stated or clearly implied by the Jira data.
            - If a section cannot be filled due to insufficient Jira data, write "No data available from Jira for this section." instead of making up content.
            - Every use case, requirement, and business rule MUST be traceable to specific Jira issue keys.
            - Be as thorough, detailed, and comprehensive as possible. Cover every Jira issue.
            - Output valid Markdown only. Use "# ", "## ", "### " for headings, "- " for bullets, "| " for tables.

            DOCUMENT STRUCTURE (follow exactly):

            # [Project Name]
            # Software Requirement Specification

            **Class Code:** [ClassCode]
            **Group Code:** [GroupCode]
            **Generated:** {todayDate}

            ---

            # Record of Change
            *A - Added | M - Modified | D - Deleted*
            | Effective Date | Changed Items | A / M / D | Change Description | New Version |
            (fill with Initial row, date = {todayDate}, version 1.0)

            ---

            # SIGNATURE PAGE
            ## ORIGINATOR
            | Name | Date | Role/Title |
            (fill with the provided team members, date = {todayDate})

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
            (describe what the overall use case diagram should contain based on Jira epics/stories)
            ## 3.2 Use Case Specifications
            For EACH major feature/epic/story from Jira, create a Use Case Specification table:
            | Field | Value |
            | Use-case No. | UC-X |
            | Use-case Name | ... |
            | Jira Reference | [issue keys] |
            | Priority | ... |
            | Primary Actor | ... |
            | Secondary Actor | ... |
            Then: Description, Triggers, Preconditions (PRE-X), Post Conditions (POST-X),
            Main Success Scenario (numbered steps), Alternative Scenario, Exceptions,
            Relationships, Business Rules (BR-XX references).
            Include ALL relevant Jira issues — do not skip any.

            ## 3.3 State Diagrams
            (describe relevant state diagrams derived from Jira issue statuses and workflows)
            ## 3.4 Data Flow Diagrams
            (describe relevant DFDs derived from Jira data)
            ## 3.5 Logical Data Model
            (describe ERD/schema derived from Jira features)

            # 4. NON-FUNCTIONAL REQUIREMENTS
            ## 4.1 Usability
            ## 4.2 Reliability
            ## 4.3 Performance
            ## 4.4 Reusability
            ## 4.5 Scalability
            (only include non-functional requirements that can be derived from Jira labels, components, or descriptions)

            # 5. Supporting Information
            ## 5.1 Appendices
            ## Appendix A — Business Rules Reference
            (BR-XX format, grouped by category, derived from Jira only)
            ## Appendix B — Integration Requirements
            ## Appendix C — Security Requirements
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
            Semester: {semesterName}
            Group Code: {jiraProjectKey}
            Team Members: {membersInfo}
            Project Description: {(string.IsNullOrWhiteSpace(projectDescription) ? "Not provided" : projectDescription)}
            Today's Date: {todayDate}

            === JIRA ISSUES (this is the ONLY source of truth) ===
            {issuesSummary}
            === END OF JIRA ISSUES ===

            Generate the full SRS document in Markdown now. Use ONLY the Jira issues above as the source for all requirements, use cases, and business rules. Do NOT create any information that is not present in the Jira data. Be as comprehensive and detailed as possible.
            """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _openAISettings.MaxOutputTokens,
            Temperature = 1f
        };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        var responseText = ExtractCompletionText(completion.Value);
        return responseText;
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

    private static string GetFriendlyAIErrorMessage(ClientResultException ex, bool usePaidModel)
    {
        var message = ex.Message;
        var statusCode = ex.Status;
        var modelLabel = usePaidModel ? "OpenAI (paid)" : "GitHub Models (free)";

        return statusCode switch
        {
            429 => $"The {modelLabel} API quota has been exceeded. Please check the billing and plan details for the configured API key.",
            401 => $"The {modelLabel} API key is invalid or has been revoked. Please update the API key in the configuration.",
            403 => $"Access denied by the {modelLabel} API. The API key may lack the required permissions.",
            413 => $"The request is too large for the {modelLabel} API. Try a project with fewer Jira issues, or use the paid model for higher limits.",
            500 or 502 or 503 => $"The {modelLabel} API is temporarily unavailable. Please try again later.",
            _ => $"The {modelLabel} API returned an error (HTTP {statusCode}): {message}"
        };
    }

    private static bool IsTimeoutException(Exception ex)
    {
        if (ex is TaskCanceledException or OperationCanceledException)
            return true;

        if (ex is AggregateException agg)
            return agg.InnerExceptions.Any(e => e is TaskCanceledException or OperationCanceledException);

        return false;
    }

    private static string ExtractCompletionText(ChatCompletion completion)
    {
        if (completion.Content is not { Count: > 0 })
            throw new InvalidOperationException(
                $"The AI model returned no content. Finish reason: {completion.FinishReason}");

        var text = completion.Content[0].Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException(
                $"The AI model returned empty content. Finish reason: {completion.FinishReason}. " +
                "This may happen when the model's output was fully consumed by reasoning/thinking tokens. Try again.");

        return text;
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

    private static string GetNestedString(JsonElement el, string prop, string nested, string deepNested)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Object)
            return GetNestedString(val, nested, deepNested);
        return string.Empty;
    }

    private static List<string> ExtractSubtasks(JsonElement fields)
    {
        if (!fields.TryGetProperty("subtasks", out var subtasksArray) || subtasksArray.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<string>();
        foreach (var subtask in subtasksArray.EnumerateArray())
        {
            var subKey = GetString(subtask, "key");
            var subSummary = GetNestedString(subtask, "fields", "summary");
            if (!string.IsNullOrWhiteSpace(subKey))
                results.Add($"{subKey}{(string.IsNullOrWhiteSpace(subSummary) ? "" : $" ({subSummary})")}");
        }
        return results;
    }

    private static List<string> ExtractIssueLinks(JsonElement fields)
    {
        if (!fields.TryGetProperty("issuelinks", out var linksArray) || linksArray.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<string>();
        foreach (var link in linksArray.EnumerateArray())
        {
            var linkType = GetNestedString(link, "type", "name");
            if (link.TryGetProperty("outwardIssue", out var outward) && outward.ValueKind == JsonValueKind.Object)
            {
                var outKey = GetString(outward, "key");
                var outSummary = GetNestedString(outward, "fields", "summary");
                var direction = link.TryGetProperty("type", out var t) ? GetString(t, "outward") : linkType;
                results.Add($"{direction} {outKey}{(string.IsNullOrWhiteSpace(outSummary) ? "" : $" ({outSummary})")}");
            }
            if (link.TryGetProperty("inwardIssue", out var inward) && inward.ValueKind == JsonValueKind.Object)
            {
                var inKey = GetString(inward, "key");
                var inSummary = GetNestedString(inward, "fields", "summary");
                var direction = link.TryGetProperty("type", out var t) ? GetString(t, "inward") : linkType;
                results.Add($"{direction} {inKey}{(string.IsNullOrWhiteSpace(inSummary) ? "" : $" ({inSummary})")}");
            }
        }
        return results;
    }

    private static List<string> ExtractComments(JsonElement fields)
    {
        if (!fields.TryGetProperty("comment", out var commentField))
            return [];

        JsonElement commentsArray;
        if (commentField.ValueKind == JsonValueKind.Object)
        {
            if (!commentField.TryGetProperty("comments", out commentsArray) || commentsArray.ValueKind != JsonValueKind.Array)
                return [];
        }
        else if (commentField.ValueKind == JsonValueKind.Array)
        {
            commentsArray = commentField;
        }
        else
        {
            return [];
        }

        var results = new List<string>();
        foreach (var comment in commentsArray.EnumerateArray())
        {
            var author = GetNestedString(comment, "author", "displayName");
            var created = GetString(comment, "created");
            var body = comment.TryGetProperty("body", out var bodyEl)
                ? GetAtlassianDocTextFromElement(bodyEl)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(body))
                results.Add($"[{author}, {created}]: {body}");
        }
        return results;
    }

    private static string GetAtlassianDocTextFromElement(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return string.Empty;

        if (!element.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var parts = new List<string>();
        ExtractTextFromAdf(content, parts);
        return string.Join(" ", parts);
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

    private async Task<(string? Content, string? ProjectName, string? ErrorMessage)> GenerateGithubReportContentAsync(Guid projectId, bool usePaidModel, string? modelOption)
    {
        if (usePaidModel)
        {
            if (string.IsNullOrWhiteSpace(_openAISettings.ApiKey))
                return (null, null, "OpenAI API key is not configured. Please set the ApiKey in the OpenAI configuration to use the paid model.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_modelsSettings.GitHubToken))
                return (null, null, "GitHub Models API token is not configured. Please set the GitHubToken in the GitHubModels configuration.");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
            return (null, null, "Project not found");

        var repos = (await _unitOfWork.GithubRepos.GetReposByProjectIdWithSemesterAsync(projectId)).ToList();
        if (repos.Count == 0)
            return (null, null, "No GitHub repositories found for this project");

        var repoIds = repos.Select(r => r.GithubRepoId).ToList();
        var cachedWeeklyContributions = (await _unitOfWork.WeeklyContributions.GetWithUserContributionsByRepoIdsAsync(repoIds)).ToList();

        var members = await _unitOfWork.ProjectMembers.GetMembersByProjectIdAsync(projectId);
        var memberNames = members.Select(m => m.User.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();

        var projectClass = project.Class;
        var semester = projectClass?.Semester;
        var course = projectClass?.Course;

        var inputBuilder = new StringBuilder();
        inputBuilder.AppendLine($"Project: {project.Name}");
        inputBuilder.AppendLine($"Description: {(string.IsNullOrWhiteSpace(project.Description) ? "N/A" : project.Description)}");
        inputBuilder.AppendLine($"Course: {course?.Code} - {course?.Name}");
        inputBuilder.AppendLine($"Class: {projectClass?.ClassCode ?? "N/A"}");
        inputBuilder.AppendLine($"Semester: {semester?.Name ?? "N/A"} ({semester?.StartDate:yyyy-MM-dd} to {semester?.EndDate:yyyy-MM-dd})");
        inputBuilder.AppendLine($"Team Members: {(memberNames.Count > 0 ? string.Join(", ", memberNames) : "No members listed")}");
        inputBuilder.AppendLine();

        var limitations = new List<string>();

        var reposWithContributorStats = 0;
        var reposWithCommits = 0;
        var reposWithPullRequests = 0;
        var reposWithIssues = 0;
        var reposWithActivityLogs = 0;
        var reposWithCachedWeeklyData = 0;

        foreach (var repo in repos)
        {
            var repoWeeklyContributions = cachedWeeklyContributions
                .Where(w => w.GithubRepoId == repo.GithubRepoId)
                .OrderBy(w => w.WeekStart)
                .ToList();

            var cachedContributorTotals = repoWeeklyContributions
                .SelectMany(w => w.UserContributions)
                .GroupBy(u => u.GithubUsername, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Username = g.Key,
                    Commits = g.Sum(x => x.Commits),
                    Additions = g.Sum(x => x.Additions),
                    Deletions = g.Sum(x => x.Deletions)
                })
                .OrderByDescending(x => x.Commits)
                .ToList();

            inputBuilder.AppendLine($"## Repository Metadata");
            inputBuilder.AppendLine($"- Repo: {repo.RepoOwnerName}/{repo.RepoName}");
            inputBuilder.AppendLine($"- URL: https://github.com/{repo.RepoOwnerName}/{repo.RepoName}");
            inputBuilder.AppendLine($"- Private: {repo.IsPrivate}");
            inputBuilder.AppendLine($"- Last Synced: {(repo.LastSyncedAt.HasValue ? repo.LastSyncedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}");
            inputBuilder.AppendLine($"- Cached Totals (DB): commits={repo.TotalCommits}, additions={repo.TotalAdditions}, deletions={repo.TotalDeletions}");
            inputBuilder.AppendLine();

            var token = !string.IsNullOrWhiteSpace(repo.ApiToken)
                ? repo.ApiToken
                : (!string.IsNullOrWhiteSpace(_modelsSettings.GitHubToken) ? _modelsSettings.GitHubToken : null);

            if (repo.IsPrivate && string.IsNullOrWhiteSpace(token))
                limitations.Add($"Repository {repo.RepoOwnerName}/{repo.RepoName} is private but no GitHub API token is configured; live PR/issue/commit data may be unavailable.");

            var contributorStats = await _githubApiService.GetRepositoryContributorStatsAsync(repo.RepoOwnerName, repo.RepoName, token);
            var commitActivity = await _githubApiService.GetRepositoryCommitActivityAsync(repo.RepoOwnerName, repo.RepoName, token);
            var commits = await _githubApiService.GetRepositoryCommitsAsync(repo.RepoOwnerName, repo.RepoName, 50, token);
            var pullRequests = await _githubApiService.GetRepositoryPullRequestsAsync(repo.RepoOwnerName, repo.RepoName, 40, token);
            var issues = await _githubApiService.GetRepositoryIssuesAsync(repo.RepoOwnerName, repo.RepoName, 60, token);
            var activityLogs = await _githubApiService.GetRepositoryActivityLogsAsync(repo.RepoOwnerName, repo.RepoName, 40, token);

            if (commitActivity == null)
                limitations.Add($"Could not load commit activity timeline for {repo.RepoOwnerName}/{repo.RepoName}.");
            if (contributorStats == null)
                limitations.Add($"Could not load contributor stats for {repo.RepoOwnerName}/{repo.RepoName}.");
            if (commits == null)
                limitations.Add($"Could not load commit history for {repo.RepoOwnerName}/{repo.RepoName}.");
            if (pullRequests == null)
                limitations.Add($"Could not load pull requests for {repo.RepoOwnerName}/{repo.RepoName}.");
            if (issues == null)
                limitations.Add($"Could not load issues for {repo.RepoOwnerName}/{repo.RepoName}.");
            if (activityLogs == null)
                limitations.Add($"Could not load activity logs for {repo.RepoOwnerName}/{repo.RepoName}.");

            if (repoWeeklyContributions.Count > 0)
                reposWithCachedWeeklyData++;
            if (contributorStats?.Contributors is { Count: > 0 })
                reposWithContributorStats++;
            if (commits is { Count: > 0 })
                reposWithCommits++;
            if (pullRequests is { Count: > 0 })
                reposWithPullRequests++;
            if (issues is { Count: > 0 })
                reposWithIssues++;
            if (activityLogs is { Count: > 0 })
                reposWithActivityLogs++;

            inputBuilder.AppendLine("### Contributors");
            if (contributorStats?.Contributors is { Count: > 0 })
            {
                foreach (var contributor in contributorStats.Contributors.OrderByDescending(c => c.TotalCommits))
                {
                    inputBuilder.AppendLine($"- {contributor.Login}: commits={contributor.TotalCommits}, additions={contributor.TotalAdditions}, deletions={contributor.TotalDeletions}");
                }
            }
            else if (cachedContributorTotals.Count > 0)
            {
                inputBuilder.AppendLine("- Live GitHub contributor stats unavailable. Using cached sync data:");
                foreach (var contributor in cachedContributorTotals)
                {
                    inputBuilder.AppendLine($"- {contributor.Username}: commits={contributor.Commits}, additions={contributor.Additions}, deletions={contributor.Deletions}");
                }
            }
            else if (repo.RepoContributors.Count > 0)
            {
                inputBuilder.AppendLine("- Live/cached contribution metrics unavailable. Known contributors from repository mapping:");
                foreach (var contributor in repo.RepoContributors)
                {
                    inputBuilder.AppendLine($"- {contributor.GithubUsername} ({contributor.User?.Name ?? "Unmapped user"})");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No contributor stats available.");
            }

            inputBuilder.AppendLine();
            inputBuilder.AppendLine("### Commit History");
            if (commits is { Count: > 0 })
            {
                foreach (var commit in commits.OrderByDescending(c => c.Date))
                {
                    var message = Truncate(commit.Message.Replace('\n', ' '), 180);
                    var lineDelta = (commit.Additions > 0 || commit.Deletions > 0)
                        ? $"+{commit.Additions} / -{commit.Deletions}"
                        : "line changes: N/A from commit list endpoint";
                    inputBuilder.AppendLine($"- [{commit.Date:yyyy-MM-dd}] {commit.AuthorLogin} | {lineDelta} | {message}");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No commit history available.");
            }

            inputBuilder.AppendLine();
            inputBuilder.AppendLine("### Pull Requests");
            if (pullRequests is { Count: > 0 })
            {
                foreach (var pr in pullRequests.OrderByDescending(p => p.CreatedAt))
                {
                    inputBuilder.AppendLine(
                        $"- PR #{pr.Number}: {Truncate(pr.Title, 140)} | author={pr.AuthorLogin} | state={pr.State} | merged={pr.IsMerged} | reviews={pr.ReviewCount} (approved={pr.ApprovedReviewCount}, changes-requested={pr.ChangesRequestedReviewCount}) | comments={pr.CommentCount} | review-comments={pr.ReviewCommentCount}");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No pull request data available.");
            }

            inputBuilder.AppendLine();
            inputBuilder.AppendLine("### Issues");
            if (issues is { Count: > 0 })
            {
                foreach (var issue in issues.OrderByDescending(i => i.CreatedAt))
                {
                    var assignees = issue.Assignees.Count > 0 ? string.Join(", ", issue.Assignees) : "Unassigned";
                    inputBuilder.AppendLine(
                        $"- Issue #{issue.Number}: {Truncate(issue.Title, 140)} | state={issue.State} | author={issue.AuthorLogin} | assignees={assignees} | created={issue.CreatedAt:yyyy-MM-dd} | closed={(issue.ClosedAt.HasValue ? issue.ClosedAt.Value.ToString("yyyy-MM-dd") : "N/A")}");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No issue data available.");
            }

            inputBuilder.AppendLine();
            inputBuilder.AppendLine("### Activity Logs");
            if (activityLogs is { Count: > 0 })
            {
                foreach (var activity in activityLogs.OrderByDescending(a => a.CreatedAt))
                {
                    inputBuilder.AppendLine($"- [{activity.CreatedAt:yyyy-MM-dd}] {activity.ActorLogin} | {activity.EventType}{(string.IsNullOrWhiteSpace(activity.Action) ? string.Empty : $" ({activity.Action})")}");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No activity logs available.");
            }

            inputBuilder.AppendLine();
            inputBuilder.AppendLine("### Commit Activity Timeline (Weekly)");
            if (commitActivity is { Count: > 0 })
            {
                foreach (var week in commitActivity.OrderBy(w => w.Timestamp))
                {
                    var weekDate = DateTimeOffset.FromUnixTimeSeconds(week.Timestamp).UtcDateTime;
                    inputBuilder.AppendLine($"- Week starting {weekDate:yyyy-MM-dd}: commits={week.Total}");
                }
            }
            else if (repoWeeklyContributions.Count > 0)
            {
                inputBuilder.AppendLine("- Live GitHub weekly activity unavailable. Using cached sync data:");
                foreach (var week in repoWeeklyContributions)
                {
                    inputBuilder.AppendLine($"- Week starting {week.WeekStart:yyyy-MM-dd}: commits={week.TotalCommits}");
                }
            }
            else
            {
                inputBuilder.AppendLine("- No weekly commit activity available.");
            }

            inputBuilder.AppendLine();
        }

        inputBuilder.AppendLine("## Data Coverage Summary");
        inputBuilder.AppendLine($"- Repositories analyzed: {repos.Count}");
        inputBuilder.AppendLine($"- Repositories with live contributor stats: {reposWithContributorStats}/{repos.Count}");
        inputBuilder.AppendLine($"- Repositories with commit history: {reposWithCommits}/{repos.Count}");
        inputBuilder.AppendLine($"- Repositories with pull request data: {reposWithPullRequests}/{repos.Count}");
        inputBuilder.AppendLine($"- Repositories with issue data: {reposWithIssues}/{repos.Count}");
        inputBuilder.AppendLine($"- Repositories with activity logs: {reposWithActivityLogs}/{repos.Count}");
        inputBuilder.AppendLine($"- Repositories with cached weekly sync data: {reposWithCachedWeeklyData}/{repos.Count}");
        inputBuilder.AppendLine();

        if (limitations.Count > 0)
        {
            inputBuilder.AppendLine("## Data Collection Limitations");
            foreach (var limitation in limitations.Distinct())
                inputBuilder.AppendLine($"- {limitation}");
            inputBuilder.AppendLine();
        }

        var report = await GenerateGithubReportWithAIAsync(project.Name, inputBuilder.ToString(), usePaidModel, modelOption);
        return (report, project.Name, null);
    }

    private async Task<string> GenerateGithubReportWithAIAsync(string projectName, string githubDataSummary, bool usePaidModel, string? modelOption)
    {
        var client = usePaidModel
            ? new OpenAIClient(
                new ApiKeyCredential(_openAISettings.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(_openAISettings.Endpoint),
                    NetworkTimeout = TimeSpan.FromSeconds(_openAISettings.NetworkTimeoutInSeconds)
                })
            : new OpenAIClient(
                new ApiKeyCredential(_modelsSettings.GitHubToken),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(_modelsSettings.Endpoint)
                });

        var selectedModel = usePaidModel
            ? ResolvePaidModelName(_openAISettings.DefaultGithubReportModelOption, modelOption)
            : _modelsSettings.ModelName;

        var chatClient = client.GetChatClient(selectedModel);

        const string systemPrompt = """
            You are an expert software project analyst.

            Your task is to analyze a GitHub repository using the provided data (commits, pull requests, issues, contributors, and activity logs) and generate a clear, objective, and structured report about team contribution and project progress.

            CRITICAL CONSTRAINTS:
            - Use ONLY facts and metrics explicitly present in the provided input data.
            - Do NOT use prior knowledge about this repository, team, framework, course, timeline, or project context.
            - Do NOT infer dates, project stage, contributor identity, or workflow details if the required evidence is missing in input.
            - For any unavailable metric, write "N/A (data unavailable)".
            - If a section has insufficient evidence, explicitly state that limitation in that section.
            - Never fabricate numbers.

            ## INPUT

            You will be given:

            * Repository metadata
            * List of contributors
            * Commit history (author, date, message, lines added/removed)
            * Pull requests (author, status, reviews)
            * Issues (opened/closed, assignees)
            * Any additional activity logs

            ## YOUR OBJECTIVES

            ### 1. Contribution Analysis

            * Identify all contributors
            * Rank contributors based on:

              * Number of commits
              * Lines of code added/removed
              * Pull requests created and merged
              * Issues handled
            * Clearly identify:

              * Top contributor(s) (who did the most work)
              * Lowest contributor(s) (who did the least work)
            * Detect any imbalance in workload

            ### 2. Work Distribution

            * Analyze how work is distributed across the team
            * Highlight:

              * If one person dominates development
              * If contributions are evenly spread
            * Mention inactive or minimally active members

            ### 3. Progress & Timeline

            * Analyze commit frequency over time
            * Identify:

              * Active development periods
              * Slowdowns or inactivity
            * Estimate overall project progress (e.g., early stage, mid development, near completion)

            ### 4. Code & Collaboration Quality

            * Evaluate:

              * Quality of commit messages (clear vs vague)
              * PR review activity (collaboration level)
            * Detect potential issues:

              * Large unreviewed PRs
              * Many small/noisy commits
              * Lack of issue tracking

            ### 5. Key Insights

            * Provide 3–5 important insights about the project
            * Highlight risks (e.g., dependency on one developer, lack of reviews)

            ### 6. Recommendations

            * Suggest actionable improvements:

              * Better workload distribution
              * Improved collaboration practices
              * Development workflow improvements

            ## OUTPUT FORMAT

            Your report MUST follow this structure:

            ### 📊 Project Overview

            (brief summary)

            ### 👥 Contribution Ranking

            (table or ranked list)

            ### ⚖️ Work Distribution Analysis

            (clear explanation)

            ### 📈 Progress & Activity Timeline

            (insights over time)

            ### 🧠 Key Insights

            (bullet points)

            ### 🚀 Recommendations

            (actionable suggestions)

            ## RULES

            * Be objective and data-driven
            * Do NOT assume missing data
            * Use clear and concise language
            * Avoid unnecessary fluff
            * Highlight important metrics with numbers

            If data is incomplete, explicitly state limitations.
            """;

        var userPromptPrefix = $"""
            Project Name: {projectName}

            === GITHUB DATA START ===
            """;

        const string userPromptSuffix = """
            === GITHUB DATA END ===

            Generate the report now in Markdown following the exact required structure.
            For any missing metric, write: N/A (data unavailable).
            """;

        var inputTokenLimit = usePaidModel ? 50000 : MaxInputTokens;
        var trimmedData = TrimTextToFit(systemPrompt, userPromptPrefix, userPromptSuffix, githubDataSummary, inputTokenLimit);
        var userPrompt = $"{userPromptPrefix}\n{trimmedData}\n{userPromptSuffix}";

        var completion = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions
            {
                Temperature = 0.2f,
                MaxOutputTokenCount = usePaidModel ? _openAISettings.MaxOutputTokens : 4096
            });

        return ExtractCompletionText(completion.Value);
    }

    private string ResolvePaidModelName(string defaultOption, string? modelOption)
    {
        var option = string.IsNullOrWhiteSpace(modelOption)
            ? defaultOption
            : modelOption.Trim().ToLowerInvariant();

        return option switch
        {
            "fast" => _openAISettings.FastModelName,
            "balanced" => _openAISettings.BalancedModelName,
            "quality" => _openAISettings.QualityModelName,
            _ => defaultOption.ToLowerInvariant() switch
            {
                "fast" => _openAISettings.FastModelName,
                "balanced" => _openAISettings.BalancedModelName,
                "quality" => _openAISettings.QualityModelName,
                _ => _openAISettings.ModelName
            }
        };
    }

    private static string TrimTextToFit(string systemPrompt, string userPromptPrefix, string userPromptSuffix, string content, int maxInputTokens)
    {
        var fixedChars = systemPrompt.Length + userPromptPrefix.Length + userPromptSuffix.Length;
        var fixedTokens = (fixedChars / TokensPerChar) + PromptOverheadTokens;
        var availableTokensForContent = maxInputTokens - fixedTokens;

        if (availableTokensForContent < 200)
            availableTokensForContent = 200;

        var maxContentChars = availableTokensForContent * TokensPerChar;

        if (content.Length <= maxContentChars)
            return content;

        var truncated = content[..maxContentChars];
        var lastNewline = truncated.LastIndexOf('\n');
        if (lastNewline > maxContentChars / 2)
            truncated = truncated[..lastNewline];

        return truncated + "\n[... additional data truncated to fit model input limit ...]";
    }
}
