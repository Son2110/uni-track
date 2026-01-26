using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Domain.Enums;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class AccessRequestRepository : GenericRepository<AccessRequest>, IAccessRequestRepository
{
    public AccessRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AccessRequest>> GetRequestsByRequesterIdAsync(Guid requesterId)
    {
        return await _dbSet
            .Include(ar => ar.Requester)
            .Include(ar => ar.Project)
            .Where(ar => ar.RequesterId == requesterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AccessRequest>> GetRequestsByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Include(ar => ar.Requester)
            .Include(ar => ar.Project)
            .Where(ar => ar.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AccessRequest>> GetRequestsByStatusAsync(AccessRequestStatus status)
    {
        return await _dbSet
            .Include(ar => ar.Requester)
            .Include(ar => ar.Project)
            .Where(ar => ar.Status == status)
            .ToListAsync();
    }

    public async Task<AccessRequest?> GetPendingRequestAsync(Guid requesterId, Guid projectId)
    {
        return await _dbSet
            .Include(ar => ar.Requester)
            .Include(ar => ar.Project)
            .FirstOrDefaultAsync(ar => ar.RequesterId == requesterId 
                && ar.ProjectId == projectId 
                && ar.Status == AccessRequestStatus.Pending);
    }
}
