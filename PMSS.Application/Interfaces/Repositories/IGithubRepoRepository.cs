using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IGithubRepoRepository : IGenericRepository<GithubRepo>
{
    Task<GithubRepo?> GetByOwnerAndNameAsync(string ownerName, string repoName);
    Task<IEnumerable<GithubRepo>> GetReposByProjectIdAsync(Guid projectId);
}
