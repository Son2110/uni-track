using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class GithubRepoRepository : GenericRepository<GithubRepo>, IGithubRepoRepository
{
    public GithubRepoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<GithubRepo?> GetByOwnerAndNameAsync(string ownerName, string repoName)
    {
        return await _dbSet
            .Include(gr => gr.Project)
            .FirstOrDefaultAsync(gr => gr.RepoOwnerName == ownerName && gr.RepoName == repoName);
    }

    public async Task<IEnumerable<GithubRepo>> GetReposByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Include(gr => gr.Project)
                .ThenInclude(p => p.Class)
                    .ThenInclude(c => c.Course)
            .Include(gr => gr.RepoContributors)
                .ThenInclude(rc => rc.User)
            .Where(gr => gr.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GithubRepo>> GetReposByCourseIdAsync(Guid courseId)
    {
        return await _dbSet
            .Include(gr => gr.Project)
                .ThenInclude(p => p.Class)
                    .ThenInclude(c => c.Course)
            .Include(gr => gr.RepoContributors)
                .ThenInclude(rc => rc.User)
            .Where(gr => gr.Project.Class.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GithubRepo>> GetReposByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(gr => gr.Project)
                .ThenInclude(p => p.Class)
                    .ThenInclude(c => c.Course)
            .Include(gr => gr.Project)
                .ThenInclude(p => p.ProjectMembers)
            .Include(gr => gr.RepoContributors)
                .ThenInclude(rc => rc.User)
            .Where(gr => gr.Project.ProjectMembers.Any(pm => pm.UserId == userId) ||
                        gr.RepoContributors.Any(rc => rc.UserId == userId))
            .ToListAsync();
    }

    public async Task<GithubRepo?> GetRepoWithDetailsAsync(Guid repoId)
    {
        return await _dbSet
            .Include(gr => gr.Project)
                .ThenInclude(p => p.Class)
                    .ThenInclude(c => c.Course)
            .Include(gr => gr.Project)
                .ThenInclude(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
            .Include(gr => gr.RepoContributors)
                .ThenInclude(rc => rc.User)
            .FirstOrDefaultAsync(gr => gr.GithubRepoId == repoId);
    }

    public async Task<IEnumerable<GithubRepo>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(gr => gr.Project)
                    .ThenInclude(p => p.Class)
                        .ThenInclude(c => c.Course)
                .Include(gr => gr.RepoContributors)
                    .ThenInclude(rc => rc.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<GithubRepo>> GetReposByProjectIdWithSemesterAsync(Guid projectId)
        {
            return await _dbSet
                .Include(gr => gr.Project)
                    .ThenInclude(p => p.Class)
                        .ThenInclude(c => c.Course)
                .Include(gr => gr.Project)
                    .ThenInclude(p => p.Class)
                        .ThenInclude(c => c.Semester)
                .Include(gr => gr.RepoContributors)
                    .ThenInclude(rc => rc.User)
                .Where(gr => gr.ProjectId == projectId)
                .ToListAsync();
        }
    }

