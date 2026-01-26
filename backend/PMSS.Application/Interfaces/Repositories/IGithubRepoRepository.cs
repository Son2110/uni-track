using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IGithubRepoRepository : IGenericRepository<GithubRepo>
{
    Task<GithubRepo?> GetByOwnerAndNameAsync(string ownerName, string repoName);
    Task<IEnumerable<GithubRepo>> GetReposByProjectIdAsync(Guid projectId);
    Task<IEnumerable<GithubRepo>> GetReposByCourseIdAsync(Guid courseId);
    Task<IEnumerable<GithubRepo>> GetReposByUserIdAsync(Guid userId);
    Task<GithubRepo?> GetRepoWithDetailsAsync(Guid repoId);
}
