using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

public class SrsGeneratorService : ISrsGeneratorService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<SrsGeneratorService> _logger;
    private readonly string _modelId;
    private readonly string _storagePath;

    public SrsGeneratorService(IConfiguration configuration, ILogger<SrsGeneratorService> logger)
    {
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured in appsettings.json");

        _modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";
        _storagePath = configuration["SrsStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "generated-srs");

        var endpoint = configuration["OpenAI:Endpoint"];

        if (!string.IsNullOrEmpty(endpoint))
        {
            var options = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpoint) };
            var client = new OpenAI.OpenAIClient(new ApiKeyCredential(apiKey), options);
            _chatClient = client.GetChatClient(_modelId);
        }
        else
        {
            _chatClient = new ChatClient(_modelId, new ApiKeyCredential(apiKey));
        }
    }

    public async Task<string> GenerateSrsFromJiraAsync(string jiraIssuesJson, string projectName)
    {
        _logger.LogInformation("Generating SRS for project '{ProjectName}' using model '{ModelId}'", projectName, _modelId);

        var prompt = BuildSrsPrompt(jiraIssuesJson, projectName);

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert Software Requirements Analyst. Generate complete SRS documents in Markdown format following the exact template structure provided. Output ONLY the Markdown content — no code fences, no preamble."),
                new UserChatMessage(prompt)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            var srsContent = completion.Content[0].Text ?? string.Empty;

            srsContent = CleanMarkdownOutput(srsContent);

            _logger.LogInformation("SRS generation completed for project '{ProjectName}'. Output length: {Length} chars", projectName, srsContent.Length);

            return srsContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SRS for project '{ProjectName}'", projectName);
            throw new InvalidOperationException($"AI generation failed: {ex.Message}", ex);
        }
    }

    public async Task<string> SaveSrsToFileAsync(string srsContent, Guid projectId, string projectName)
    {
        Directory.CreateDirectory(_storagePath);

        var sanitizedName = string.Join("_", projectName.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"SRS_{sanitizedName}_{projectId:N}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md";
        var filePath = Path.Combine(_storagePath, fileName);

        await File.WriteAllTextAsync(filePath, srsContent);

        _logger.LogInformation("SRS saved to file: {FilePath}", filePath);
        return fileName;
    }

    public string? GetSrsFilePath(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        // Prevent path traversal
        var fullPath = Path.GetFullPath(filePath);
        var fullStoragePath = Path.GetFullPath(_storagePath);
        if (!fullPath.StartsWith(fullStoragePath, StringComparison.OrdinalIgnoreCase))
            return null;

        return File.Exists(fullPath) ? fullPath : null;
    }

    public string[] GetSrsFilesByProject(Guid projectId)
    {
        if (!Directory.Exists(_storagePath))
            return [];

        return Directory.GetFiles(_storagePath, $"SRS_*_{projectId:N}_*.md")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .ToArray()!;
    }

    private static string BuildSrsPrompt(string jiraIssuesJson, string projectName)
    {
        return $"""
            Analyze the Jira issues JSON data below and generate a complete Software Requirement Specification (SRS) document in Markdown format.

            ## Project Information
            - **Project Name:** {projectName}
            - **Date:** {DateTime.UtcNow:MMMM dd, yyyy}

            ## Instructions
            1. Analyze all Jira issues (summary, description, status) from the JSON data
            2. Group related issues into functional modules/features
            3. Generate a complete SRS following the EXACT template structure below
            4. For each Jira issue, create a corresponding Use Case with the exact table format shown
            5. Infer non-functional requirements from the issues where possible
            6. Write in a professional, clear, and detailed manner
            7. Output ONLY the Markdown content — no code fences, no preamble

            ## REQUIRED SRS Template (follow this EXACTLY)

            # {projectName}

            # Software Requirement Specification

            ---

            **Class Code:** [Infer from project or use N/A]
            **Group Code:** [Infer from project or use N/A]

            **[Location], {DateTime.UtcNow:MMMM dd, yyyy}**

            ---

            # Record of Change

            *A - Added | M - Modified | D - Deleted*

            | Effective Date | Changed Items | A / M / D | Change Description | New Version |
            |----------------|----------------|-----------|--------------------|-------------|
            | {DateTime.UtcNow:MMMM dd, yyyy} | Initial | A | Auto-generated from Jira issues | 1.0 |

            ---

            # 1. Introduction

            ## 1.1 Purpose
            (Describe the purpose of this SRS document for the project)

            ## 1.2 Definitions, Acronyms
            (List all abbreviations and technical terms)

            ## 1.3 References
            - IEEE Std 830-1998
            - (Add domain-specific references)

            ---

            # 2. Overall Description

            ## 2.1 Product Perspective
            (Describe the system context — standalone or part of larger system)

            ## 2.2 Business Process
            (Provide high-level workflows supported by the system, derived from Jira issues)

            ## 2.3 User Classes
            (Define user classes with Goals, Tasks, Technical Expertise)

            ---

            # 3. FUNCTIONAL REQUIREMENTS

            ## 3.1 Use Case Diagram
            (Describe the overall use case diagram textually)

            ## 3.2 Use Case Specifications

            For EACH use case, use this EXACT table format:

            ### USE CASE SPECIFICATION

            | Field | Value |
            |--------|---------|
            | Use-case No. | UC-X |
            | Use-case Version | 1.0 |
            | Use-case Name | [Name derived from Jira issue] |
            | Author | Auto-generated |
            | Date | {DateTime.UtcNow:MMMM dd, yyyy} |
            | Priority | [From Jira priority or infer] |
            | Primary Actor | [User/Admin] |
            | Secondary Actor | [System/Database] |

            **Description:**
            [Brief summary from Jira issue]

            **Triggers:**
            [Event that starts use case]

            **Preconditions:**
            - PRE-1. [Condition]

            **Post Conditions:**
            - POST-1. [Result]

            ### Main Success Scenario
            1. [Step 1]
            2. [Step 2]

            ### Alternative Scenario
            **1.1 [Scenario Name]**
            - [Description]

            ### Exceptions
            **E1 — [Error Name]**
            - [Description]

            ### Relationships
            [Dependencies on other use cases]

            ### Business Rules
            [Reference BR-XX]

            ---

            ## 3.3 State Diagrams
            (Describe state diagrams for entities with lifecycle complexity)

            ## 3.4 Data Flow Diagrams
            (Describe DFDs for critical processes)

            ## 3.5 Logical Data Model
            (Describe ERD or schema)

            ---

            # 4. NON-FUNCTIONAL REQUIREMENTS

            ## 4.1 Usability
            - [Requirements]

            ## 4.2 Reliability
            - [Requirements]

            ## 4.3 Performance
            - [Requirements]

            ## 4.4 Reusability
            - [Requirements]

            ## 4.5 Scalability
            - [Requirements]

            ---

            # 5. Supporting Information

            ## Appendix A — Business Rules Reference
            (Use format BR-XX for each rule, grouped by category)

            ## Appendix B — Integration Requirements
            (List external systems or APIs)

            ## Appendix C — Security Requirements
            (List required protocols and standards)

            ---

            ## Jira Issues JSON Data
            ```json
            {jiraIssuesJson}
            ```

            Generate the complete SRS document now.
            """;
    }

    private static string CleanMarkdownOutput(string content)
    {
        content = content.Trim();

        if (content.StartsWith("```markdown", StringComparison.OrdinalIgnoreCase))
        {
            content = content["```markdown".Length..];
        }
        else if (content.StartsWith("```"))
        {
            content = content[3..];
        }

        if (content.EndsWith("```"))
        {
            content = content[..^3];
        }

        return content.Trim();
    }
}
