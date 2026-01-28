using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IClassRepository : IGenericRepository<Class>
{
    Task<Class?> GetByIdWithDetailsAsync(Guid classId);
    Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(Guid teacherId);
    Task<IEnumerable<Class>> GetClassesBySemesterIdAsync(Guid semesterId);
    Task<IEnumerable<Class>> GetClassesByCourseIdAsync(Guid courseId);
    Task<Class?> GetClassBySemesterCourseAndSectionAsync(Guid semesterId, Guid courseId, string classCode);
}
