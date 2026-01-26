using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IJiraConfigRepository : IGenericRepository<JiraConfig>
{
    Task<JiraConfig?> GetByProjectIdAsync(Guid projectId);
    Task<JiraConfig?> GetActiveConfigByProjectIdAsync(Guid projectId);
}
