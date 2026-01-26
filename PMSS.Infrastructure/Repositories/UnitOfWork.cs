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
    public IJiraConfigRepository JiraConfigs { get; }
    public IAccessRequestRepository AccessRequests { get; }

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
        IJiraConfigRepository jiraConfigs,
        IAccessRequestRepository accessRequests)
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
        JiraConfigs = jiraConfigs;
        AccessRequests = accessRequests;
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
