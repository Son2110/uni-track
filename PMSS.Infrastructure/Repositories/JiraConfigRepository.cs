using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class JiraConfigRepository : GenericRepository<JiraConfig>, IJiraConfigRepository
{
    public JiraConfigRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<JiraConfig?> GetByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Include(jc => jc.Project)
            .FirstOrDefaultAsync(jc => jc.ProjectId == projectId);
    }

    public async Task<JiraConfig?> GetActiveConfigByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Include(jc => jc.Project)
            .FirstOrDefaultAsync(jc => jc.ProjectId == projectId && jc.IsActive);
    }
}
