using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class UserWeeklyContributionRepository : GenericRepository<UserWeeklyContribution>, IUserWeeklyContributionRepository
{
    public UserWeeklyContributionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserWeeklyContribution>> GetByWeeklyContributionIdAsync(Guid weeklyContributionId)
    {
        return await _context.UserWeeklyContributions
            .Where(uwc => uwc.WeeklyContributionId == weeklyContributionId)
            .Include(uwc => uwc.User)
            .OrderBy(uwc => uwc.GithubUsername)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserWeeklyContribution>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserWeeklyContributions
            .Where(uwc => uwc.UserId == userId)
            .Include(uwc => uwc.WeeklyContribution)
            .OrderBy(uwc => uwc.WeeklyContribution.WeekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserWeeklyContribution>> GetByGithubUsernameAsync(string githubUsername)
    {
        return await _context.UserWeeklyContributions
            .Where(uwc => uwc.GithubUsername == githubUsername)
            .Include(uwc => uwc.WeeklyContribution)
            .Include(uwc => uwc.User)
            .OrderBy(uwc => uwc.WeeklyContribution.WeekStart)
            .ToListAsync();
    }

    public async Task<UserWeeklyContribution?> GetByWeeklyContributionAndUsernameAsync(Guid weeklyContributionId, string githubUsername)
    {
        return await _context.UserWeeklyContributions
            .FirstOrDefaultAsync(uwc => 
                uwc.WeeklyContributionId == weeklyContributionId && 
                uwc.GithubUsername == githubUsername);
    }

    public async Task<IEnumerable<UserWeeklyContribution>> GetByWeeklyContributionIdsAsync(IEnumerable<Guid> weeklyContributionIds)
    {
        var idSet = weeklyContributionIds.ToHashSet();
        return await _context.UserWeeklyContributions
            .Where(uwc => idSet.Contains(uwc.WeeklyContributionId))
            .Include(uwc => uwc.User)
            .Include(uwc => uwc.WeeklyContribution)
            .OrderBy(uwc => uwc.WeeklyContribution.WeekStart)
            .ThenBy(uwc => uwc.GithubUsername)
            .ToListAsync();
    }
}
