namespace PMSS.Domain.Entities;

public class GithubRepo
{
    public Guid GithubRepoId { get; set; }
    public Guid ProjectId { get; set; }
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? ApiToken { get; set; }
    
    /// <summary>
    /// Total commits across all contributors (cached from GitHub API)
    /// </summary>
    public int TotalCommits { get; set; }
    
    /// <summary>
    /// Total line additions across all contributors (cached from GitHub API)
    /// </summary>
    public int TotalAdditions { get; set; }
    
    /// <summary>
    /// Total line deletions across all contributors (cached from GitHub API)
    /// </summary>
    public int TotalDeletions { get; set; }
    
    /// <summary>
    /// Last time GitHub data was synchronized
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<RepoContributor> RepoContributors { get; set; } = new List<RepoContributor>();
    public virtual ICollection<WeeklyContribution> WeeklyContributions { get; set; } = new List<WeeklyContribution>();
}
