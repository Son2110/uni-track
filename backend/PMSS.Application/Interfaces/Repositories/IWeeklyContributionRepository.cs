using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IWeeklyContributionRepository : IGenericRepository<WeeklyContribution>
{
    Task<IEnumerable<WeeklyContribution>> GetByRepoIdAsync(Guid githubRepoId);
    Task<IEnumerable<WeeklyContribution>> GetByRepoIdsAsync(IEnumerable<Guid> githubRepoIds);
    Task<IEnumerable<WeeklyContribution>> GetByRepoIdAndDateRangeAsync(Guid githubRepoId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<WeeklyContribution>> GetByRepoIdsAndDateRangeAsync(IEnumerable<Guid> githubRepoIds, DateTime startDate, DateTime endDate);
    Task<WeeklyContribution?> GetByRepoIdAndWeekAsync(Guid githubRepoId, long weekTimestamp);
    Task<IEnumerable<WeeklyContribution>> GetWithUserContributionsAsync(Guid githubRepoId);
    Task<IEnumerable<WeeklyContribution>> GetWithUserContributionsByRepoIdsAsync(IEnumerable<Guid> githubRepoIds);
    Task DeleteByRepoIdAsync(Guid githubRepoId);
}
