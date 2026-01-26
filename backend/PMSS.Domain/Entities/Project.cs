namespace PMSS.Domain.Entities;

public class Project
{
    public Guid ProjectId { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public virtual ICollection<GithubRepo> GithubRepos { get; set; } = new List<GithubRepo>();
    public virtual JiraConfig? JiraConfig { get; set; }
    public virtual ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
}
