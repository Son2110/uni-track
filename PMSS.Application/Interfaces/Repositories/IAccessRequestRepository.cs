using PMSS.Domain.Entities;
using PMSS.Domain.Enums;

namespace PMSS.Application.Interfaces.Repositories;

public interface IAccessRequestRepository : IGenericRepository<AccessRequest>
{
    Task<IEnumerable<AccessRequest>> GetRequestsByRequesterIdAsync(Guid requesterId);
    Task<IEnumerable<AccessRequest>> GetRequestsByProjectIdAsync(Guid projectId);
    Task<IEnumerable<AccessRequest>> GetRequestsByStatusAsync(AccessRequestStatus status);
    Task<AccessRequest?> GetPendingRequestAsync(Guid requesterId, Guid projectId);
}
