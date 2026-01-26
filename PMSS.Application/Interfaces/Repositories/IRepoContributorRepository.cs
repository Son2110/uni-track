using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IRepoContributorRepository : IGenericRepository<RepoContributor>
{
    Task<RepoContributor?> GetContributorAsync(Guid githubRepoId, string githubUsername);
    Task<IEnumerable<RepoContributor>> GetContributorsByRepoIdAsync(Guid githubRepoId);
}
