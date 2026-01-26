using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class ClassRepository : GenericRepository<Class>, IClassRepository
{
    public ClassRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Class?> GetByIdWithDetailsAsync(Guid classId)
    {
        return await _dbSet
            .Include(c => c.Semester)
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(Guid teacherId)
    {
        return await _dbSet
            .Include(c => c.Semester)
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesBySemesterIdAsync(Guid semesterId)
    {
        return await _dbSet
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Where(c => c.SemesterId == semesterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesByCourseIdAsync(Guid courseId)
    {
        return await _dbSet
            .Include(c => c.Semester)
            .Include(c => c.Teacher)
            .Where(c => c.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<Class?> GetClassBySemesterCourseAndSectionAsync(Guid semesterId, Guid courseId, string section)
    {
        return await _dbSet
            .Include(c => c.Semester)
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.SemesterId == semesterId && c.CourseId == courseId && c.Section == section);
    }
}
