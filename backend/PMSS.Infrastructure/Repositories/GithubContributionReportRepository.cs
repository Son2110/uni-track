using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class GithubContributionReportRepository : GenericRepository<GithubContributionReport>, IGithubContributionReportRepository
{
    public GithubContributionReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GithubContributionReport>> GetByProjectIdAsync(Guid projectId, int take = 20)
    {
        return await _dbSet
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<GithubContributionReport?> GetLatestByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<GithubContributionReport?> GetByIdWithProjectAsync(Guid reportId)
    {
        return await _dbSet
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.GithubContributionReportId == reportId);
    }
}