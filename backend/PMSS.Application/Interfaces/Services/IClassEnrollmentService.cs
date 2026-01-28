using PMSS.Application.DTOs.ClassEnrollment;
using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

public interface IClassEnrollmentService
{
    Task<ApiResponse<PagedResult<ClassEnrollmentDto>>> GetAllEnrollmentsAsync(ClassEnrollmentFilterParams filterParams);
    Task<ApiResponse<ClassEnrollmentDto>> GetEnrollmentAsync(Guid classId, Guid userId);
    Task<ApiResponse<IEnumerable<ClassEnrollmentDto>>> GetEnrollmentsByClassIdAsync(Guid classId);
    Task<ApiResponse<IEnumerable<ClassEnrollmentDto>>> GetEnrollmentsByUserIdAsync(Guid userId);
    Task<ApiResponse<ClassEnrollmentDto>> EnrollStudentAsync(CreateClassEnrollmentDto dto);
    Task<ApiResponse<List<ClassEnrollmentDto>>> BulkEnrollStudentsAsync(BulkEnrollmentDto dto);
    Task<ApiResponse<bool>> UnenrollStudentAsync(Guid classId, Guid userId);
    Task<ApiResponse<int>> GetEnrollmentCountByClassIdAsync(Guid classId);
}
