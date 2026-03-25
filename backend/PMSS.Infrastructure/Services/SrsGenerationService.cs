using System.Text.Json;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Srs;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

public class SrsGenerationService : ISrsGenerationService
{
    private readonly IJiraApiService _jiraApiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SrsGenerationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SrsGenerationService(
        IJiraApiService jiraApiService,
        IUnitOfWork unitOfWork,
        ILogger<SrsGenerationService> logger)
    {
        _jiraApiService = jiraApiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<SrsDocumentDto>> GenerateSrsAsync(Guid projectId)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null)
                return ApiResponse<SrsDocumentDto>.ErrorResponse("Project not found");

            var jiraConfig = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);
            if (jiraConfig == null || !jiraConfig.IsActive)
                return ApiResponse<SrsDocumentDto>.ErrorResponse("No active Jira configuration found for this project");

            string rawJson;
            try
            {
                rawJson = await _jiraApiService.FetchRawJiraIssuesAsync(projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Jira issues for project {ProjectId}", projectId);
                return ApiResponse<SrsDocumentDto>.ErrorResponse("Failed to fetch Jira issues", ex.Message);
            }

            var jiraResponse = JsonSerializer.Deserialize<JsonElement>(rawJson, JsonOptions);
            var issues = ParseIssues(jiraResponse);

            var projectClass = project.Class;
            var course = projectClass?.Course;

            var srs = new SrsDocumentDto
            {
                GeneratedAt = DateTime.UtcNow,
                ProjectInfo = new SrsProjectInfoDto
                {
                    ProjectName = project.Name,
                    JiraProjectKey = jiraConfig.ProjectKey,
                    CourseName = course?.Name ?? string.Empty,
                    ClassName = projectClass != null ? $"{course?.Code}-{projectClass.ClassCode}" : string.Empty,
                    TotalIssues = issues.Count
                },
                Introduction = BuildIntroduction(project.Name, project.Description, issues),
                OverallDescription = BuildOverallDescription(project.Name, issues),
                SpecificRequirements = BuildSpecificRequirements(issues),
                TraceabilityMatrix = BuildTraceabilityMatrix(issues)
            };

            return ApiResponse<SrsDocumentDto>.SuccessResponse(srs, "SRS document generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SRS for project {ProjectId}", projectId);
            return ApiResponse<SrsDocumentDto>.ErrorResponse("Error generating SRS document", ex.Message);
        }
    }

    private static List<ParsedIssue> ParseIssues(JsonElement root)
    {
        var issues = new List<ParsedIssue>();

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

        return issues;
    }

    private static SrsIntroductionDto BuildIntroduction(string projectName, string? description, List<ParsedIssue> issues)
    {
        var components = issues.SelectMany(i => i.Components).Distinct().ToList();

        return new SrsIntroductionDto
        {
            Purpose = $"This Software Requirements Specification (SRS) document describes the functional and non-functional requirements for the {projectName} system. It is generated from Jira project data following IEEE/ISO/IEC 29148 standard.",
            Scope = !string.IsNullOrWhiteSpace(description)
                ? description
                : $"The {projectName} system encompasses {issues.Count} requirements across {components.Count} component(s).",
            Definitions = components.Count > 0
                ? components.Select(c => $"{c} — System component/module").ToList()
                : []
        };
    }

    private static SrsOverallDescriptionDto BuildOverallDescription(string projectName, List<ParsedIssue> issues)
    {
        var epics = issues.Where(i => i.IssueType.Equals("Epic", StringComparison.OrdinalIgnoreCase)).ToList();
        var stories = issues.Where(i => i.IssueType.Equals("Story", StringComparison.OrdinalIgnoreCase)).ToList();
        var bugs = issues.Where(i => i.IssueType.Equals("Bug", StringComparison.OrdinalIgnoreCase)).ToList();

        return new SrsOverallDescriptionDto
        {
            ProductPerspective = $"{projectName} is a software system with {epics.Count} epic(s), {stories.Count} story/stories, and {bugs.Count} bug(s) tracked in Jira.",
            ProductFunctions = epics.Select(e => $"{e.Key}: {e.Summary}").ToList(),
            UserCharacteristics = issues.Select(i => i.Assignee).Where(a => !string.IsNullOrEmpty(a)).Distinct().Select(a => $"Contributor: {a}").ToList(),
            Constraints = bugs.Select(b => $"[{b.Key}] {b.Summary} (Priority: {b.Priority})").ToList(),
            Assumptions = issues.SelectMany(i => i.FixVersions).Distinct().Select(v => $"Target release: {v}").ToList()
        };
    }

    private static SrsSpecificRequirementsDto BuildSpecificRequirements(List<ParsedIssue> issues)
    {
        var epics = issues.Where(i => i.IssueType.Equals("Epic", StringComparison.OrdinalIgnoreCase)).ToList();
        var childIssues = issues.Where(i => !i.IssueType.Equals("Epic", StringComparison.OrdinalIgnoreCase)).ToList();

        var nfrLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "performance", "security", "scalability", "usability", "reliability",
            "availability", "maintainability", "portability", "nfr", "non-functional"
        };

        var nonFunctional = childIssues.Where(i => i.Labels.Any(l => nfrLabels.Contains(l))).ToList();
        var functional = childIssues.Except(nonFunctional).ToList();

        var groups = new List<SrsRequirementGroupDto>();
        var counter = 1;

        foreach (var epic in epics)
        {
            var epicChildren = functional.Where(i => i.ParentKey == epic.Key).ToList();
            var ungrouped = epicChildren.Count == 0
                ? functional.Where(i => string.IsNullOrEmpty(i.ParentKey) && i.Components.Intersect(epic.Components).Any()).ToList()
                : epicChildren;

            groups.Add(new SrsRequirementGroupDto
            {
                GroupName = epic.Summary,
                GroupDescription = epic.Description,
                Source = epic.Key,
                Requirements = ungrouped.Select(i => ToRequirementDto(i, ref counter)).ToList()
            });

            functional = functional.Except(ungrouped).ToList();
        }

        if (functional.Count > 0)
        {
            groups.Add(new SrsRequirementGroupDto
            {
                GroupName = "Other Functional Requirements",
                Requirements = functional.Select(i => ToRequirementDto(i, ref counter)).ToList()
            });
        }

        var nfrCounter = 1;
        var nfrList = nonFunctional.Select(i =>
        {
            var dto = ToRequirementDto(i, ref nfrCounter);
            dto.Id = $"NFR-{nfrCounter - 1:D3}";
            return dto;
        }).ToList();

        return new SrsSpecificRequirementsDto
        {
            FunctionalRequirements = groups,
            NonFunctionalRequirements = nfrList
        };
    }

    private static SrsRequirementDto ToRequirementDto(ParsedIssue issue, ref int counter)
    {
        var dto = new SrsRequirementDto
        {
            Id = $"FR-{counter:D3}",
            JiraKey = issue.Key,
            Title = issue.Summary,
            Description = issue.Description,
            Priority = issue.Priority,
            Status = issue.Status,
            Type = issue.IssueType,
            Assignee = issue.Assignee,
            Component = issue.Components.FirstOrDefault(),
            Labels = issue.Labels,
            Created = issue.Created,
            Updated = issue.Updated,
            Dependencies = issue.LinkedIssueKeys
        };
        counter++;
        return dto;
    }

    private static List<SrsTraceabilityEntryDto> BuildTraceabilityMatrix(List<ParsedIssue> issues)
    {
        var counter = 1;
        return issues
            .Where(i => !i.IssueType.Equals("Epic", StringComparison.OrdinalIgnoreCase))
            .Select(i => new SrsTraceabilityEntryDto
            {
                RequirementId = $"REQ-{counter++:D3}",
                JiraKey = i.Key,
                Title = i.Summary,
                Status = i.Status,
                Priority = i.Priority,
                Type = i.IssueType,
                Dependencies = i.LinkedIssueKeys
            })
            .ToList();
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

    private static DateTime GetDateTime(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(val.GetString(), out var dt))
                return dt;
        }
        return DateTime.MinValue;
    }

    private static List<string> GetStringArray(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Array)
            return val.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!).ToList();
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
