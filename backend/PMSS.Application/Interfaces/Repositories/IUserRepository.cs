using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByGithubUsernameAsync(string githubUsername);
    Task<bool> ExistsByEmailAsync(string email);
}
