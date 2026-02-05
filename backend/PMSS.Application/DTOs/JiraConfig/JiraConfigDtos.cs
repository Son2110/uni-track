using System.ComponentModel.DataAnnotations;

namespace PMSS.Application.DTOs.JiraConfig;

public class JiraConfigDto
{
    public Guid JiraConfigId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string JiraUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiTokenMasked { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
}

public class CreateJiraConfigDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [Url(ErrorMessage = "Please provide a valid Jira URL")]
    public string JiraUrl { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ApiToken { get; set; } = string.Empty;

    [Required]
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateJiraConfigDto
{
    [Url(ErrorMessage = "Please provide a valid Jira URL")]
    public string? JiraUrl { get; set; }

    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string? Email { get; set; }

    public string? ApiToken { get; set; }

    public string? ProjectKey { get; set; }

    public bool? IsActive { get; set; }
}

public class JiraConfigFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ProjectId { get; set; }
    public bool? IsActive { get; set; }
}
