using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface ISemesterRepository : IGenericRepository<Semester>
{
    Task<Semester?> GetByNameAsync(string name);
    Task<IEnumerable<Semester>> GetActivesSemestersAsync();
}
