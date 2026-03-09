namespace PMSS.Domain.Entities;

/// <summary>
/// Junction table representing a user's contributions for a specific week.
/// This enables Many-to-Many relationship between Users and WeeklyContributions
/// while storing the contribution data (commits, additions, deletions) per user per week.
/// </summary>
public class UserWeeklyContribution
{
    public Guid UserWeeklyContributionId { get; set; }
    public Guid WeeklyContributionId { get; set; }
    public string GithubUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional link to system user (may be null if contributor is not a registered user)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Number of commits by this user in this week
    /// </summary>
    public int Commits { get; set; }
    
    /// <summary>
    /// Number of line additions by this user in this week
    /// </summary>
    public int Additions { get; set; }
    
    /// <summary>
    /// Number of line deletions by this user in this week
    /// </summary>
    public int Deletions { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual WeeklyContribution WeeklyContribution { get; set; } = null!;
    public virtual User? User { get; set; }
}
