using Microsoft.EntityFrameworkCore;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.Infrastructure.Repositories;

public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Semester?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<IEnumerable<Semester>> GetActivesSemestersAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(s => s.StartDate <= today && s.EndDate >= today)
            .ToListAsync();
    }
}
