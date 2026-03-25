namespace PMSS.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    ISemesterRepository Semesters { get; }
    IUserRepository Users { get; }
    ICourseRepository Courses { get; }
    IClassRepository Classes { get; }
    IClassEnrollmentRepository ClassEnrollments { get; }
    IProjectRepository Projects { get; }
    IProjectMemberRepository ProjectMembers { get; }
    IGithubRepoRepository GithubRepos { get; }
    IRepoContributorRepository RepoContributors { get; }
    IWeeklyContributionRepository WeeklyContributions { get; }
    IUserWeeklyContributionRepository UserWeeklyContributions { get; }
    IJiraConfigRepository JiraConfigs { get; }
    IAccessRequestRepository AccessRequests { get; }
    INotificationRepository Notifications { get; }
    IGithubContributionReportRepository GithubContributionReports { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
