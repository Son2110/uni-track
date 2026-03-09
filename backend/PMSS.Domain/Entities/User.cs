using PMSS.Domain.Enums;

namespace PMSS.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GithubUsername { get; set; }
    public string? GithubEmail { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Class> TaughtClasses { get; set; } = new List<Class>();
    public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public virtual ICollection<RepoContributor> RepoContributors { get; set; } = new List<RepoContributor>();
    public virtual ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
    
    /// <summary>
    /// User's weekly contributions across all repositories
    /// </summary>
    public virtual ICollection<UserWeeklyContribution> WeeklyContributions { get; set; } = new List<UserWeeklyContribution>();
}
