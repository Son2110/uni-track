namespace PMSS.Domain.Entities;

public class GithubRepo
{
    public Guid GithubRepoId { get; set; }
    public Guid ProjectId { get; set; }
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? ApiToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<RepoContributor> RepoContributors { get; set; } = new List<RepoContributor>();
}
