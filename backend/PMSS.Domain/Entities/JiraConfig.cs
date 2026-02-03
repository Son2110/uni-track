namespace PMSS.Domain.Entities;

public class JiraConfig
{
    public Guid JiraConfigId { get; set; }
    public Guid ProjectId { get; set; }
    public string JiraUrl { get; set; } = string.Empty;
    // Email removed - will be auto-filled from authenticated user
    public string ApiToken { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Track who created this config (Admin) - nullable for backward compatibility
    public Guid? CreatedByUserId { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
