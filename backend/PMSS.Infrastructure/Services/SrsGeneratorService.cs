using System.ClientModel;
using System.Text.Json;
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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

        // Parse Jira issues into structured objects for enhanced prompt context
        var parsedIssues = ParseJiraIssues(jiraIssuesJson);
        var prompt = BuildSrsPrompt(jiraIssuesJson, projectName, parsedIssues);

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

    private string BuildSrsPrompt(string jiraIssuesJson, string projectName, List<ParsedIssue> parsedIssues)
    {
        var today = DateTime.UtcNow.ToString("MMMM dd, yyyy");

        // Build issue classification summary from parsed data
        var epics = parsedIssues.Where(i => i.IssueType.Equals("Epic", StringComparison.OrdinalIgnoreCase)).ToList();
        var stories = parsedIssues.Where(i => i.IssueType.Equals("Story", StringComparison.OrdinalIgnoreCase)).ToList();
        var tasks = parsedIssues.Where(i => i.IssueType.Equals("Task", StringComparison.OrdinalIgnoreCase)).ToList();
        var bugs = parsedIssues.Where(i => i.IssueType.Equals("Bug", StringComparison.OrdinalIgnoreCase)).ToList();
        var components = parsedIssues.SelectMany(i => i.Components).Distinct().ToList();
        var assignees = parsedIssues.Where(i => !string.IsNullOrEmpty(i.Assignee)).Select(i => i.Assignee).Distinct().ToList();

        var issueClassification = $"""
            ## Issue Classification Summary
            - **Total Issues:** {parsedIssues.Count}
            - **Epics:** {epics.Count} | **Stories:** {stories.Count} | **Tasks:** {tasks.Count} | **Bugs:** {bugs.Count}
            - **Components/Modules:** {string.Join(", ", components.Any() ? components : ["Not specified"])}
            - **Team Members:** {string.Join(", ", assignees.Any() ? assignees : ["Not assigned"])}
            - **Priority Levels:** {string.Join(", ", parsedIssues.Select(i => i.Priority).Distinct().Where(p => !string.IsNullOrEmpty(p)).DefaultIfEmpty("Not specified"))}
            
            """;

        return $"""
            You are a senior Software Requirements Analyst with 15+ years of experience writing IEEE 830-compliant SRS documents.

            ## YOUR TASK
            Analyze the Jira issues JSON data provided at the end and generate a **complete, production-ready** Software Requirement Specification (SRS) document for the project "{projectName}".

            ## ISSUE OVERVIEW
            {issueClassification}

            ## CRITICAL RULES
            - You MUST fill in ALL sections with **real, meaningful content** derived from the Jira issues — NEVER leave placeholders like [Description], [Name], or (Describe...).
            - If a Jira issue lacks detail, **infer reasonable requirements** based on the issue summary and common software patterns.
            - Every Use Case MUST have at least 5 detailed steps in the Main Success Scenario.
            - Every Use Case MUST have at least 1 Alternative Scenario and 1 Exception with full descriptions.
            - Preconditions and Post Conditions MUST have at least 2 items each.
            - Non-functional requirements MUST be **specific and measurable** (e.g., "Response time < 2 seconds for 95% of requests" NOT just "fast response time").
            - Business Rules MUST be concrete and numbered (BR-01, BR-02, etc.) with real descriptions.
            - Write **extensive, detailed content** — a complete SRS should be at least 3000+ words.
            - Output ONLY raw Markdown — no code fences, no ```markdown wrapper, no preamble text.

            ## REQUIRED DOCUMENT STRUCTURE (follow EXACTLY)

            ---

            # {projectName}

            # Software Requirement Specification

            ---

            **Class Code:** N/A
            **Group Code:** N/A

            **{today}**

            ---

            # Record of Change

            *A - Added | M - Modified | D - Deleted*

            | Effective Date | Changed Items | A / M / D | Change Description | New Version |
            |----------------|----------------|-----------|--------------------|-------------|
            | {today} | All Sections | A | Initial SRS auto-generated from Jira issues | 1.0 |

            ---

            # SIGNATURE PAGE

            ## ORIGINATOR

            | Name | Date | Role/Title |
            |--------|--------|-------------|
            | AI Generator | {today} | SRS Auto-Generator |

            ## REVIEWERS

            | Name | Date | Role |
            |--------|--------|--------|
            | (Pending Review) | — | Project Stakeholder |

            ---

            # TABLE OF CONTENTS

            (Generate a real table of contents listing all sections and subsections with section numbers)

            ---

            # 1. Introduction

            ## 1.1 Purpose
            Write 3-5 sentences describing the purpose of this SRS document. Mention:
            - What system this document specifies
            - Who the intended audience is (developers, testers, stakeholders)
            - What decisions this document supports

            ## 1.2 Definitions, Acronyms
            Extract ALL technical terms, abbreviations, and domain-specific vocabulary from the Jira issues. Format each as:
            - **TERM:** Full definition with context

            Include at least: SRS, API, UI/UX, CRUD, and any domain-specific terms from the issues.

            ## 1.3 References
            - IEEE Std 830-1998 — IEEE Recommended Practice for Software Requirements Specifications
            - List any technologies, frameworks, or standards implied by the Jira issues

            ---

            # 2. Overall Description

            ## 2.1 Product Perspective
            Write 5-8 sentences describing:
            - What the system does at a high level
            - Whether it's standalone or part of a larger ecosystem
            - What external systems it interacts with (APIs, databases, third-party services)
            - The technology context (web app, mobile app, etc.) inferred from the issues

            ## 2.2 Business Process
            For EACH distinct business workflow identified from the Jira issues, describe:
            - **Process Name:** Clear name
            - **Description:** 2-3 sentences explaining the workflow
            - **Actors involved:** Who participates
            - **Key steps:** Numbered list of high-level steps

            ## 2.3 User Classes
            For EACH user role identified or inferred from the Jira issues:

            ### [Role Name] (e.g., Administrator, Student, Mentor)
            - **Goals:** What they want to achieve (list 3+ goals)
            - **Tasks:** Specific actions they perform (list 3+ tasks)
            - **Technical Expertise:** Beginner / Intermediate / Advanced
            - **Frequency of Use:** Daily / Weekly / Occasionally

            ---

            # 3. FUNCTIONAL REQUIREMENTS

            ## 3.1 Use Case Diagram
            Provide a **textual description** of the use case diagram:
            - List all actors on the left
            - List all use cases grouped by module/feature
            - Describe relationships (include, extend, generalization)
            - Use clear formatting to make it readable

            ## 3.2 Use Case Specifications

            For EVERY Jira issue (or group of closely related issues), create a complete Use Case using this EXACT format:

            ---

            ### USE CASE SPECIFICATION

            | Field | Value |
            |--------|---------|
            | Use-case No. | UC-[number] |
            | Use-case Version | 1.0 |
            | Use-case Name | [Descriptive name from Jira issue summary] |
            | Author | Auto-generated from Jira |
            | Date | {today} |
            | Priority | [High/Medium/Low — infer from Jira priority or issue type] |
            | Primary Actor | [Specific actor, e.g., Student, Admin, System] |
            | Secondary Actor | [System, Database, External API, etc.] |

            **Description:**
            [3-5 sentences describing what this use case accomplishes and why it's important]

            **Triggers:**
            [Specific event or action that initiates this use case]

            **Preconditions:**
            - PRE-1. [Specific condition that must be true]
            - PRE-2. [Another condition]
            - PRE-3. [Another condition if applicable]

            **Post Conditions:**
            - POST-1. [Specific result/state after success]
            - POST-2. [Another result]

            ### Main Success Scenario
            1. [Actor] navigates to / initiates [specific action]
            2. System displays / loads [specific UI or data]
            3. [Actor] enters / selects [specific input]
            4. System validates [what is validated]
            5. System processes [what happens]
            6. System confirms [success feedback]
            7. [Final state description]

            ### Alternative Scenario
            **[Step#]a. [Scenario Name]**
            - [Detailed description of what happens differently]
            - [How the system responds]
            - [How flow continues or returns to main scenario]

            ### Exceptions
            **E1 — [Specific Error Name]**
            - Condition: [When this error occurs]
            - System response: [What the system does]
            - User recovery: [How the user recovers]

            ### Relationships
            - Extends: [UC-X if applicable]
            - Includes: [UC-Y if applicable]
            - Depends on: [UC-Z if applicable]

            ### Business Rules
            - BR-[XX]: [Specific rule that applies]

            ---

            (REPEAT the above for EVERY use case — do NOT skip any Jira issues)

            ## 3.3 State Diagrams
            For entities with lifecycle states (e.g., Project Status, Task Status, User Account), describe:
            - Entity name
            - All possible states
            - Transitions between states with trigger events
            - Use text format: State1 --[event]--> State2

            ## 3.4 Data Flow Diagrams
            Describe the data flow for the 2-3 most critical processes:
            - External entities (actors, systems)
            - Processes (numbered)
            - Data stores
            - Data flows with descriptions

            ## 3.5 Logical Data Model
            Describe the key entities and their relationships:
            - Entity name
            - Key attributes (name, type, constraints)
            - Relationships (one-to-many, many-to-many, etc.)

            ---

            # 4. NON-FUNCTIONAL REQUIREMENTS

            ## 4.1 Usability
            - USR-1: [Specific measurable usability requirement, e.g., "New users shall complete registration in under 3 minutes"]
            - USR-2: [e.g., "The system shall support mobile responsive design for screens 320px and above"]
            - USR-3: [e.g., "All form validation errors shall display within 500ms of user input"]

            ## 4.2 Reliability
            - REL-1: [e.g., "System uptime shall be 99.5% measured monthly"]
            - REL-2: [e.g., "Automated database backups shall occur every 24 hours"]
            - REL-3: [e.g., "Mean Time To Recovery (MTTR) shall not exceed 4 hours"]

            ## 4.3 Performance
            - PER-1: [e.g., "API response time shall be < 2 seconds for 95% of requests under normal load"]
            - PER-2: [e.g., "System shall support 500 concurrent users without degradation"]
            - PER-3: [e.g., "Page load time shall be < 3 seconds on 4G mobile connection"]

            ## 4.4 Reusability
            - REU-1: [e.g., "System shall use modular architecture with independent service components"]
            - REU-2: [e.g., "API endpoints shall follow RESTful conventions for third-party integration"]

            ## 4.5 Scalability
            - SCA-1: [e.g., "System architecture shall support horizontal scaling to handle 10x user growth"]
            - SCA-2: [e.g., "Database shall support partitioning for tables exceeding 1M rows"]

            ---

            # 5. Supporting Information

            ## 5.1 Appendices

            ## Appendix A — Business Rules Reference

            Group ALL business rules by category. Each rule must be specific and actionable:

            ### User Authentication & Authorization
            - BR-01: [Concrete rule, e.g., "Users must verify email before accessing system features"]
            - BR-02: [e.g., "Session timeout after 30 minutes of inactivity"]

            ### Data Validation
            - BR-XX: [e.g., "Email format must follow RFC 5322 standard"]

            ### Business Logic
            - BR-XX: [Rules specific to the domain from Jira issues]

            ### Data Privacy & Security
            - BR-XX: [e.g., "Passwords must be hashed using bcrypt with minimum 12 rounds"]

            ## Appendix B — Integration Requirements
            List ALL external systems, APIs, and third-party services mentioned or implied in the Jira issues:
            - System name, purpose, integration method (REST API, webhook, etc.)

            ## Appendix C — Security Requirements
            - Encryption standards (TLS 1.2+, AES-256, etc.)
            - Authentication method (JWT, OAuth 2.0, etc.)
            - Authorization model (RBAC, ABAC)
            - Compliance requirements (if applicable)

            ---

            **{projectName}**

            ---

            ## Jira Issues JSON Data (INPUT — analyze this thoroughly)
            ```json
            {jiraIssuesJson}
            ```

            IMPORTANT REMINDERS:
            - Fill EVERY section with real content, not placeholders
            - Create a Use Case for EVERY Jira issue or logical group
            - Each Use Case needs 5+ steps, alternatives, and exceptions
            - Non-functional requirements must be specific and measurable
            - Business rules must be concrete with BR-XX numbering
            - Output raw Markdown only — no code fences wrapping the entire document
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

    #region JSON Parsing Helpers (from DucAn)

    /// <summary>
    /// Parses raw Jira JSON into structured ParsedIssue objects for better data extraction.
    /// This improves reliability when feeding issues to the AI for SRS generation.
    /// </summary>
    private List<ParsedIssue> ParseJiraIssues(string rawJson)
    {
        var issues = new List<ParsedIssue>();

        try
        {
            var root = JsonSerializer.Deserialize<JsonElement>(rawJson, JsonOptions);
            if (!root.TryGetProperty("issues", out var issuesArray))
                return issues;

            foreach (var issue in issuesArray.EnumerateArray())
            {
                var key = issue.GetProperty("key").GetString() ?? string.Empty;
                var fields = issue.GetProperty("fields");

                var parsed = new ParsedIssue
                {
                    Key = key,
                    Summary = GetString(fields, "summary"),
                    Description = GetAtlassianDocText(fields, "description"),
                    Status = GetNestedString(fields, "status", "name"),
                    IssueType = GetNestedString(fields, "issuetype", "name"),
                    Priority = GetNestedString(fields, "priority", "name"),
                    Assignee = GetNestedString(fields, "assignee", "displayName"),
                    Created = GetDateTime(fields, "created"),
                    Updated = GetDateTime(fields, "updated"),
                    ParentKey = GetNestedString(fields, "parent", "key"),
                    Labels = GetStringArray(fields, "labels"),
                    Components = GetNamedArray(fields, "components"),
                    FixVersions = GetNamedArray(fields, "fixVersions"),
                    LinkedIssueKeys = GetLinkedIssueKeys(fields)
                };

                issues.Add(parsed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing Jira issues JSON; continuing with raw data");
        }

        return issues;
    }

    /// <summary>
    /// Extracts a simple string value from a JSON element.
    /// </summary>
    private static string GetString(JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String
            ? val.GetString() ?? string.Empty
            : string.Empty;
    }

    /// <summary>
    /// Extracts a nested string value (e.g., status.name).
    /// </summary>
    private static string GetNestedString(JsonElement el, string prop, string nested)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Object)
            return GetString(val, nested);
        return string.Empty;
    }

    /// <summary>
    /// Parses an ISO 8601 DateTime from a JSON element.
    /// </summary>
    private static DateTime GetDateTime(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(val.GetString(), out var dt))
                return dt;
        }
        return DateTime.MinValue;
    }

    /// <summary>
    /// Extracts an array of simple string values.
    /// </summary>
    private static List<string> GetStringArray(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Array)
            return val.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!).ToList();
        return [];
    }

    /// <summary>
    /// Extracts an array of named objects (objects with a "name" property).
    /// </summary>
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

    /// <summary>
    /// Extracts linked Jira issue keys from the issuelinks array.
    /// </summary>
    private static List<string> GetLinkedIssueKeys(JsonElement fields)
    {
        var keys = new List<string>();
        if (!fields.TryGetProperty("issuelinks", out var links) || links.ValueKind != JsonValueKind.Array)
            return keys;

        foreach (var link in links.EnumerateArray())
        {
            if (link.TryGetProperty("outwardIssue", out var outward) && outward.ValueKind == JsonValueKind.Object)
                keys.Add(GetString(outward, "key"));
            if (link.TryGetProperty("inwardIssue", out var inward) && inward.ValueKind == JsonValueKind.Object)
                keys.Add(GetString(inward, "key"));
        }

        return keys.Where(k => !string.IsNullOrEmpty(k)).ToList();
    }

    /// <summary>
    /// Extracts plain text from Atlassian Document Format (ADF) description fields.
    /// Jira's description format is structured JSON; this extracts readable text.
    /// </summary>
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

    /// <summary>
    /// Recursively extracts text nodes from Atlassian Document Format.
    /// </summary>
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

    /// <summary>
    /// Represents a parsed Jira issue with structured fields for easier manipulation.
    /// </summary>
    private sealed class ParsedIssue
    {
        public string Key { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Assignee { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string? ParentKey { get; set; }
        public List<string> Labels { get; set; } = [];
        public List<string> Components { get; set; } = [];
        public List<string> FixVersions { get; set; } = [];
        public List<string> LinkedIssueKeys { get; set; } = [];
    }
}
