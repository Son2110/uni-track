using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface IClassEnrollmentRepository : IGenericRepository<ClassEnrollment>
{
    Task<ClassEnrollment?> GetEnrollmentAsync(Guid classId, Guid userId);
    Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId);
    Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByUserIdAsync(Guid userId);
}
