using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class ClassEnrollmentRepository : GenericRepository<ClassEnrollment>, IClassEnrollmentRepository
{
    public ClassEnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ClassEnrollment?> GetEnrollmentAsync(Guid classId, Guid userId)
    {
        return await _dbSet
            .Include(ce => ce.Class)
            .Include(ce => ce.User)
            .Include(ce => ce.Course)
            .FirstOrDefaultAsync(ce => ce.ClassId == classId && ce.UserId == userId);
    }

    public async Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId)
    {
        return await _dbSet
            .Include(ce => ce.Class)
            .Include(ce => ce.User)
            .Include(ce => ce.Course)
            .Where(ce => ce.ClassId == classId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(ce => ce.Class)
            .Include(ce => ce.User)
            .Include(ce => ce.Course)
            .Where(ce => ce.UserId == userId)
            .ToListAsync();
    }
}
