namespace PMSS.Domain.Entities;

/// <summary>
/// Represents a weekly time period for a GitHub repository.
/// One GithubRepo has many WeeklyContributions (one per week).
/// Contains aggregated totals for the week across all contributors.
/// </summary>
public class WeeklyContribution
{
    public Guid WeeklyContributionId { get; set; }
    public Guid GithubRepoId { get; set; }
    
    /// <summary>
    /// Unix timestamp representing the start of the week (Sunday)
    /// </summary>
    public long WeekTimestamp { get; set; }
    
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    
    /// <summary>
    /// Total commits for this week across all contributors
    /// </summary>
    public int TotalCommits { get; set; }
    
    /// <summary>
    /// Total additions for this week across all contributors
    /// </summary>
    public int TotalAdditions { get; set; }
    
    /// <summary>
    /// Total deletions for this week across all contributors
    /// </summary>
    public int TotalDeletions { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual GithubRepo GithubRepo { get; set; } = null!;
    
    /// <summary>
    /// User contributions for this week (many users can participate in each week)
    /// </summary>
    public virtual ICollection<UserWeeklyContribution> UserContributions { get; set; } = new List<UserWeeklyContribution>();
}
