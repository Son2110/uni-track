namespace PMSS.Domain.Entities;

public class RepoContributor
{
    public Guid GithubRepoId { get; set; }
    public string GithubUsername { get; set; } = string.Empty;
    public string? GithubEmail { get; set; }
    public Guid? UserId { get; set; }
    public DateTime AddedAt { get; set; }

    public virtual GithubRepo GithubRepo { get; set; } = null!;
    public virtual User? User { get; set; }
}
