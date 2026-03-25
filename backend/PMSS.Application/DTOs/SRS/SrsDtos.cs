namespace PMSS.Application.DTOs.Srs;

public class SrsDocumentDto
{
    public SrsProjectInfoDto ProjectInfo { get; set; } = new();
    public SrsIntroductionDto Introduction { get; set; } = new();
    public SrsOverallDescriptionDto OverallDescription { get; set; } = new();
    public SrsSpecificRequirementsDto SpecificRequirements { get; set; } = new();
    public List<SrsTraceabilityEntryDto> TraceabilityMatrix { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public class SrsProjectInfoDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string JiraProjectKey { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int TotalIssues { get; set; }
}

public class SrsIntroductionDto
{
    public string Purpose { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public List<string> Definitions { get; set; } = [];
}

public class SrsOverallDescriptionDto
{
    public string ProductPerspective { get; set; } = string.Empty;
    public List<string> ProductFunctions { get; set; } = [];
    public List<string> UserCharacteristics { get; set; } = [];
    public List<string> Constraints { get; set; } = [];
    public List<string> Assumptions { get; set; } = [];
}

public class SrsSpecificRequirementsDto
{
    public List<SrsRequirementGroupDto> FunctionalRequirements { get; set; } = [];
    public List<SrsRequirementDto> NonFunctionalRequirements { get; set; } = [];
}

public class SrsRequirementGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? GroupDescription { get; set; }
    public string? Source { get; set; }
    public List<SrsRequirementDto> Requirements { get; set; } = [];
}

public class SrsRequirementDto
{
    public string Id { get; set; } = string.Empty;
    public string JiraKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public string? Component { get; set; }
    public List<string> Labels { get; set; } = [];
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public List<string> Dependencies { get; set; } = [];
}

public class SrsTraceabilityEntryDto
{
    public string RequirementId { get; set; } = string.Empty;
    public string JiraKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = [];
}
