using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class WeeklyContributionRepository : GenericRepository<WeeklyContribution>, IWeeklyContributionRepository
{
    public WeeklyContributionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeeklyContribution>> GetByRepoIdAsync(Guid githubRepoId)
    {
        return await _context.WeeklyContributions
            .Where(wc => wc.GithubRepoId == githubRepoId)
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeeklyContribution>> GetByRepoIdsAsync(IEnumerable<Guid> githubRepoIds)
    {
        var repoIdSet = githubRepoIds.ToHashSet();
        return await _context.WeeklyContributions
            .Where(wc => repoIdSet.Contains(wc.GithubRepoId))
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeeklyContribution>> GetByRepoIdAndDateRangeAsync(Guid githubRepoId, DateTime startDate, DateTime endDate)
    {
        return await _context.WeeklyContributions
            .Where(wc => wc.GithubRepoId == githubRepoId && wc.WeekStart >= startDate && wc.WeekEnd <= endDate)
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeeklyContribution>> GetByRepoIdsAndDateRangeAsync(IEnumerable<Guid> githubRepoIds, DateTime startDate, DateTime endDate)
    {
        var repoIdSet = githubRepoIds.ToHashSet();
        return await _context.WeeklyContributions
            .Where(wc => repoIdSet.Contains(wc.GithubRepoId) && wc.WeekStart >= startDate && wc.WeekEnd <= endDate)
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task<WeeklyContribution?> GetByRepoIdAndWeekAsync(Guid githubRepoId, long weekTimestamp)
    {
        return await _context.WeeklyContributions
            .FirstOrDefaultAsync(wc => 
                wc.GithubRepoId == githubRepoId && 
                wc.WeekTimestamp == weekTimestamp);
    }

    public async Task<IEnumerable<WeeklyContribution>> GetWithUserContributionsAsync(Guid githubRepoId)
    {
        return await _context.WeeklyContributions
            .Where(wc => wc.GithubRepoId == githubRepoId)
            .Include(wc => wc.UserContributions)
                .ThenInclude(uwc => uwc.User)
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeeklyContribution>> GetWithUserContributionsByRepoIdsAsync(IEnumerable<Guid> githubRepoIds)
    {
        var repoIdSet = githubRepoIds.ToHashSet();
        return await _context.WeeklyContributions
            .Where(wc => repoIdSet.Contains(wc.GithubRepoId))
            .Include(wc => wc.UserContributions)
                .ThenInclude(uwc => uwc.User)
            .OrderBy(wc => wc.WeekStart)
            .ToListAsync();
    }

    public async Task DeleteByRepoIdAsync(Guid githubRepoId)
    {
        var contributions = await _context.WeeklyContributions
            .Where(wc => wc.GithubRepoId == githubRepoId)
            .ToListAsync();
        
        _context.WeeklyContributions.RemoveRange(contributions);
    }
}
