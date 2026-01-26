using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class ProjectMemberRepository : GenericRepository<ProjectMember>, IProjectMemberRepository
{
    public ProjectMemberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId)
    {
        return await _dbSet
            .Include(pm => pm.Project)
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task<IEnumerable<ProjectMember>> GetMembersByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Include(pm => pm.Project)
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectMember>> GetProjectsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(pm => pm.Project)
            .Include(pm => pm.User)
            .Where(pm => pm.UserId == userId)
            .ToListAsync();
    }
}
