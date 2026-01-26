using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<Course?> GetByCodeAsync(string code);
    Task<Course?> GetByIdWithClassesAsync(Guid courseId);
}
