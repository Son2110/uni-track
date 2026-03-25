using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IGithubContributionReportRepository : IGenericRepository<GithubContributionReport>
{
    Task<IEnumerable<GithubContributionReport>> GetByProjectIdAsync(Guid projectId, int take = 20);
    Task<GithubContributionReport?> GetLatestByProjectIdAsync(Guid projectId);
    Task<GithubContributionReport?> GetByIdWithProjectAsync(Guid reportId);
}