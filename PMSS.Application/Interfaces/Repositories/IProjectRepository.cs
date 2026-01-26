using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<IEnumerable<Project>> GetProjectsByCourseIdAsync(Guid courseId);
    Task<IEnumerable<Project>> GetProjectsByTeacherIdAsync(Guid teacherId);
}
