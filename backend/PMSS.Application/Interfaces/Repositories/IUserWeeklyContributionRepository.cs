using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IUserWeeklyContributionRepository : IGenericRepository<UserWeeklyContribution>
{
    Task<IEnumerable<UserWeeklyContribution>> GetByWeeklyContributionIdAsync(Guid weeklyContributionId);
    Task<IEnumerable<UserWeeklyContribution>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserWeeklyContribution>> GetByGithubUsernameAsync(string githubUsername);
    Task<UserWeeklyContribution?> GetByWeeklyContributionAndUsernameAsync(Guid weeklyContributionId, string githubUsername);
    Task<IEnumerable<UserWeeklyContribution>> GetByWeeklyContributionIdsAsync(IEnumerable<Guid> weeklyContributionIds);
}
