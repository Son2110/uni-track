namespace PMSS.Application.DTOs.JiraConfig;

public class JiraConfigDto
{
    public Guid JiraConfigId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string JiraUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateJiraConfigDto
{
    public Guid ProjectId { get; set; }
    public string JiraUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateJiraConfigDto
{
    public string JiraUrl { get; set; } = string.Empty;
    public string? ApiToken { get; set; }
    public bool IsActive { get; set; }
}

public class JiraConfigFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ProjectId { get; set; }
    public bool? IsActive { get; set; }
}
