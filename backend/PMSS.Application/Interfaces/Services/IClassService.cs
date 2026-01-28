using PMSS.Application.DTOs.Class;
using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

public interface IClassService
{
    Task<ApiResponse<PagedResult<ClassDto>>> GetAllClassesAsync(ClassFilterParams filterParams);
    Task<ApiResponse<ClassDto>> GetClassByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesByTeacherIdAsync(Guid teacherId);
    Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesBySemesterIdAsync(Guid semesterId);
    Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesByCourseIdAsync(Guid courseId);
    Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassDto dto);
    Task<ApiResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassDto dto);
    Task<ApiResponse<bool>> DeleteClassAsync(Guid id);
}
