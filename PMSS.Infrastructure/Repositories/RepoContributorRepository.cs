using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class RepoContributorRepository : GenericRepository<RepoContributor>, IRepoContributorRepository
{
    public RepoContributorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RepoContributor?> GetContributorAsync(Guid githubRepoId, string githubUsername)
    {
        return await _dbSet
            .Include(rc => rc.GithubRepo)
            .Include(rc => rc.User)
            .FirstOrDefaultAsync(rc => rc.GithubRepoId == githubRepoId && rc.GithubUsername == githubUsername);
    }

    public async Task<IEnumerable<RepoContributor>> GetContributorsByRepoIdAsync(Guid githubRepoId)
    {
        return await _dbSet
            .Include(rc => rc.GithubRepo)
            .Include(rc => rc.User)
            .Where(rc => rc.GithubRepoId == githubRepoId)
            .ToListAsync();
    }
}
