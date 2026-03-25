using Microsoft.EntityFrameworkCore.Storage;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public ISemesterRepository Semesters { get; }
    public IUserRepository Users { get; }
    public ICourseRepository Courses { get; }
    public IClassRepository Classes { get; }
    public IClassEnrollmentRepository ClassEnrollments { get; }
    public IProjectRepository Projects { get; }
    public IProjectMemberRepository ProjectMembers { get; }
    public IGithubRepoRepository GithubRepos { get; }
    public IRepoContributorRepository RepoContributors { get; }
    public IWeeklyContributionRepository WeeklyContributions { get; }
    public IUserWeeklyContributionRepository UserWeeklyContributions { get; }
    public IJiraConfigRepository JiraConfigs { get; }
    public IAccessRequestRepository AccessRequests { get; }
    public INotificationRepository Notifications { get; }
    public IGithubContributionReportRepository GithubContributionReports { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        ISemesterRepository semesters,
        IUserRepository users,
        ICourseRepository courses,
        IClassRepository classes,
        IClassEnrollmentRepository classEnrollments,
        IProjectRepository projects,
        IProjectMemberRepository projectMembers,
        IGithubRepoRepository githubRepos,
        IRepoContributorRepository repoContributors,
        IWeeklyContributionRepository weeklyContributions,
        IUserWeeklyContributionRepository userWeeklyContributions,
        IJiraConfigRepository jiraConfigs,
        IAccessRequestRepository accessRequests,
        INotificationRepository notifications,
        IGithubContributionReportRepository githubContributionReports)
    {
        _context = context;
        Semesters = semesters;
        Users = users;
        Courses = courses;
        Classes = classes;
        ClassEnrollments = classEnrollments;
        Projects = projects;
        ProjectMembers = projectMembers;
        GithubRepos = githubRepos;
        RepoContributors = repoContributors;
        WeeklyContributions = weeklyContributions;
        UserWeeklyContributions = userWeeklyContributions;
        JiraConfigs = jiraConfigs;
        AccessRequests = accessRequests;
        Notifications = notifications;
        GithubContributionReports = githubContributionReports;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
