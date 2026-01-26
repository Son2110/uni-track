using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IProjectMemberRepository : IGenericRepository<ProjectMember>
{
    Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId);
    Task<IEnumerable<ProjectMember>> GetMembersByProjectIdAsync(Guid projectId);
    Task<IEnumerable<ProjectMember>> GetProjectsByUserIdAsync(Guid userId);
}
