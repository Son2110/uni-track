using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Course?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<Course?> GetByIdWithClassesAsync(Guid courseId)
    {
        return await _dbSet
            .Include(c => c.Classes)
                .ThenInclude(cl => cl.Semester)
            .Include(c => c.Classes)
                .ThenInclude(cl => cl.Teacher)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }
}
