using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Class)
                .ThenInclude(c => c.Course)
            .Include(p => p.Class)
                .ThenInclude(c => c.Semester)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
    }

    public async Task<IEnumerable<Project>> GetProjectsByCourseIdAsync(Guid courseId)
    {
        return await _dbSet
            .Include(p => p.Class)
                .ThenInclude(c => c.Course)
            .Where(p => p.Class.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByTeacherIdAsync(Guid teacherId)
    {
        return await _dbSet
            .Include(p => p.Class)
                .ThenInclude(c => c.Teacher)
            .Where(p => p.Class.TeacherId == teacherId)
            .ToListAsync();
    }
}
