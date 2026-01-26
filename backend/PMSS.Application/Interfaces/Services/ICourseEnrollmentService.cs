using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.CourseEnrollment;

namespace PMSS.Application.Interfaces.Services;

public interface ICourseEnrollmentService
{
    Task<ApiResponse<PagedResult<CourseEnrollmentDto>>> GetAllEnrollmentsAsync(CourseEnrollmentFilterParams filterParams);
    Task<ApiResponse<CourseEnrollmentDto>> GetEnrollmentAsync(Guid courseId, Guid userId);
    Task<ApiResponse<CourseEnrollmentDto>> CreateEnrollmentAsync(CreateCourseEnrollmentDto dto);
    Task<ApiResponse<bool>> DeleteEnrollmentAsync(Guid courseId, Guid userId);
}
