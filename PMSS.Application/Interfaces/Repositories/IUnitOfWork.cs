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
    IJiraConfigRepository JiraConfigs { get; }
    IAccessRequestRepository AccessRequests { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
