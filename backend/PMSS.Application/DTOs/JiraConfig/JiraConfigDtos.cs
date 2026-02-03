using System.ComponentModel.DataAnnotations;

namespace PMSS.Application.DTOs.JiraConfig;

public class JiraConfigDto
{
    public Guid JiraConfigId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string JiraUrl { get; set; } = string.Empty;
    // Email removed - auto-filled from authenticated user
    public string ApiTokenMasked { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Track who created this config (nullable for backward compatibility)
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

    // Email removed - auto-filled from authenticated user

    [Required]
    public string ApiToken { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Z][A-Z0-9_]*$", ErrorMessage = "Project key must be uppercase letters, numbers, and underscores")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateJiraConfigDto
{
    [Url(ErrorMessage = "Please provide a valid Jira URL")]
    public string? JiraUrl { get; set; }

    // Email removed

    public string? ApiToken { get; set; }

    [RegularExpression(@"^[A-Z][A-Z0-9_]*$", ErrorMessage = "Project key must be uppercase letters, numbers, and underscores")]
    public string? ProjectKey { get; set; }

    public bool? IsActive { get; set; }
}

public class JiraConfigFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ProjectId { get; set; }
    public bool? IsActive { get; set; }
}
