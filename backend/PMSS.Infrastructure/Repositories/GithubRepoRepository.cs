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
            .Where(gr => gr.ProjectId == projectId)
            .ToListAsync();
    }
}
