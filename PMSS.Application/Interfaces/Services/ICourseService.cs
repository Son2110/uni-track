using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Course;

namespace PMSS.Application.Interfaces.Services;

public interface ICourseService
{
    Task<ApiResponse<PagedResult<CourseDto>>> GetAllCoursesAsync(CourseFilterParams filterParams);
    Task<ApiResponse<CourseDto>> GetCourseByIdAsync(Guid id);
    Task<ApiResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto);
    Task<ApiResponse<CourseDto>> UpdateCourseAsync(Guid id, UpdateCourseDto dto);
    Task<ApiResponse<bool>> DeleteCourseAsync(Guid id);
}
