using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Semester;

namespace PMSS.Application.Interfaces.Services;

public interface ISemesterService
{
    Task<ApiResponse<PagedResult<SemesterDto>>> GetAllSemestersAsync(SemesterFilterParams filterParams);
    Task<ApiResponse<SemesterDto>> GetSemesterByIdAsync(Guid id);
    Task<ApiResponse<SemesterDto>> CreateSemesterAsync(CreateSemesterDto dto);
    Task<ApiResponse<SemesterDto>> UpdateSemesterAsync(Guid id, UpdateSemesterDto dto);
    Task<ApiResponse<bool>> DeleteSemesterAsync(Guid id);
}
