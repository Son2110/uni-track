using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.ClassEnrollment;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class ClassEnrollmentService : IClassEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClassEnrollmentService> _logger;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ClassEnrollmentService(IUnitOfWork unitOfWork, ILogger<ClassEnrollmentService> logger, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PagedResult<ClassEnrollmentDto>>> GetAllEnrollmentsAsync(ClassEnrollmentFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all enrollments with filters: ClassId={ClassId}, UserId={UserId}, CourseId={CourseId}, SemesterId={SemesterId}",
                filterParams.ClassId, filterParams.UserId, filterParams.CourseId, filterParams.SemesterId);

            var query = (await _unitOfWork.ClassEnrollments.GetAllAsync()).AsQueryable();

            if (filterParams.ClassId.HasValue)
                query = query.Where(e => e.ClassId == filterParams.ClassId.Value);

            if (filterParams.UserId.HasValue)
                query = query.Where(e => e.UserId == filterParams.UserId.Value);

            if (filterParams.CourseId.HasValue)
                query = query.Where(e => e.CourseId == filterParams.CourseId.Value);

            if (filterParams.SemesterId.HasValue)
                query = query.Where(e => e.Class.SemesterId == filterParams.SemesterId.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(e => 
                    e.User.Name.Contains(filterParams.SearchTerm) ||
                    e.User.Email.Contains(filterParams.SearchTerm) ||
                    e.Course.Code.Contains(filterParams.SearchTerm) ||
                    e.Course.Name.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<ClassEnrollmentDto>>(items);

            var result = new PagedResult<ClassEnrollmentDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<ClassEnrollmentDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments");
            return ApiResponse<PagedResult<ClassEnrollmentDto>>.ErrorResponse("Error retrieving enrollments", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassEnrollmentDto>> GetEnrollmentAsync(Guid classId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting enrollment for ClassId={ClassId}, UserId={UserId}", classId, userId);

            var enrollment = await _unitOfWork.ClassEnrollments.GetEnrollmentAsync(classId, userId);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found for ClassId={ClassId}, UserId={UserId}", classId, userId);
                return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Enrollment not found");
            }

            return ApiResponse<ClassEnrollmentDto>.SuccessResponse(_mapper.Map<ClassEnrollmentDto>(enrollment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollment for ClassId={ClassId}, UserId={UserId}", classId, userId);
            return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Error retrieving enrollment", ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<ClassEnrollmentDto>>> GetEnrollmentsByClassIdAsync(Guid classId)
    {
        try
        {
            _logger.LogInformation("Getting enrollments for ClassId={ClassId}", classId);

            var enrollments = await _unitOfWork.ClassEnrollments.GetEnrollmentsByClassIdAsync(classId);
            var enrollmentDtos = _mapper.Map<List<ClassEnrollmentDto>>(enrollments);

            return ApiResponse<IEnumerable<ClassEnrollmentDto>>.SuccessResponse(enrollmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments for ClassId={ClassId}", classId);
            return ApiResponse<IEnumerable<ClassEnrollmentDto>>.ErrorResponse("Error retrieving enrollments", ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<ClassEnrollmentDto>>> GetEnrollmentsByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting enrollments for UserId={UserId}", userId);

            var enrollments = await _unitOfWork.ClassEnrollments.GetEnrollmentsByUserIdAsync(userId);
            var enrollmentDtos = _mapper.Map<List<ClassEnrollmentDto>>(enrollments);

            return ApiResponse<IEnumerable<ClassEnrollmentDto>>.SuccessResponse(enrollmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments for UserId={UserId}", userId);
            return ApiResponse<IEnumerable<ClassEnrollmentDto>>.ErrorResponse("Error retrieving enrollments", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassEnrollmentDto>> EnrollStudentAsync(CreateClassEnrollmentDto dto)
    {
        try
        {
            _logger.LogInformation("Enrolling student: ClassId={ClassId}, UserId={UserId}", dto.ClassId, dto.UserId);

            // Validate class exists
            var classEntity = await _unitOfWork.Classes.GetByIdWithDetailsAsync(dto.ClassId);
            if (classEntity == null)
            {
                _logger.LogWarning("Class not found: {ClassId}", dto.ClassId);
                return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Class not found");
            }

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", dto.UserId);
                return ApiResponse<ClassEnrollmentDto>.ErrorResponse("User not found");
            }

            // Check if already enrolled
            var existingEnrollment = await _unitOfWork.ClassEnrollments.GetEnrollmentAsync(dto.ClassId, dto.UserId);
            if (existingEnrollment != null)
            {
                _logger.LogWarning("Student already enrolled in class: ClassId={ClassId}, UserId={UserId}", dto.ClassId, dto.UserId);
                return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Student is already enrolled in this class");
            }

            // Check if student is already enrolled in the same course in this semester
            var userEnrollments = await _unitOfWork.ClassEnrollments.GetEnrollmentsByUserIdAsync(dto.UserId);
            var duplicateCourseEnrollment = userEnrollments.FirstOrDefault(e => 
                e.CourseId == classEntity.CourseId && 
                e.Class.SemesterId == classEntity.SemesterId);

            if (duplicateCourseEnrollment != null)
            {
                _logger.LogWarning("Student already enrolled in this course for this semester: UserId={UserId}, CourseId={CourseId}, SemesterId={SemesterId}",
                    dto.UserId, classEntity.CourseId, classEntity.SemesterId);
                return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Student is already enrolled in this course for this semester");
            }

            var enrollment = new ClassEnrollment
            {
                ClassId = dto.ClassId,
                UserId = dto.UserId,
                CourseId = classEntity.CourseId,
                EnrolledAt = DateTime.Now
            };

            await _unitOfWork.ClassEnrollments.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            enrollment = await _unitOfWork.ClassEnrollments.GetEnrollmentAsync(dto.ClassId, dto.UserId);
            _logger.LogInformation("Student enrolled successfully: ClassId={ClassId}, UserId={UserId}", dto.ClassId, dto.UserId);

            return ApiResponse<ClassEnrollmentDto>.SuccessResponse(_mapper.Map<ClassEnrollmentDto>(enrollment!), "Student enrolled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student: ClassId={ClassId}, UserId={UserId}", dto.ClassId, dto.UserId);
            return ApiResponse<ClassEnrollmentDto>.ErrorResponse("Error enrolling student", ex.Message);
        }
    }

    public async Task<ApiResponse<List<ClassEnrollmentDto>>> BulkEnrollStudentsAsync(BulkEnrollmentDto dto)
    {
        try
        {
            _logger.LogInformation("Bulk enrolling {Count} students in ClassId={ClassId}", dto.UserIds.Count, dto.ClassId);

            var successfulEnrollments = new List<ClassEnrollmentDto>();
            var errors = new List<string>();

            // Validate class exists
            var classEntity = await _unitOfWork.Classes.GetByIdWithDetailsAsync(dto.ClassId);
            if (classEntity == null)
            {
                _logger.LogWarning("Class not found: {ClassId}", dto.ClassId);
                return ApiResponse<List<ClassEnrollmentDto>>.ErrorResponse("Class not found");
            }

            foreach (var userId in dto.UserIds)
            {
                try
                {
                    var enrollDto = new CreateClassEnrollmentDto
                    {
                        ClassId = dto.ClassId,
                        UserId = userId
                    };

                    var result = await EnrollStudentAsync(enrollDto);
                    if (result.Success && result.Data != null)
                    {
                        successfulEnrollments.Add(result.Data);
                    }
                    else
                    {
                        errors.Add($"User {userId}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enrolling user {UserId}", userId);
                    errors.Add($"User {userId}: {ex.Message}");
                }
            }

            var message = $"Successfully enrolled {successfulEnrollments.Count} out of {dto.UserIds.Count} students";
            if (errors.Any())
            {
                message += $". {errors.Count} failed: {string.Join("; ", errors.Take(3))}";
            }

            _logger.LogInformation("Bulk enrollment completed: {SuccessCount}/{TotalCount} successful", 
                successfulEnrollments.Count, dto.UserIds.Count);

            return ApiResponse<List<ClassEnrollmentDto>>.SuccessResponse(successfulEnrollments, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk enrollment for ClassId={ClassId}", dto.ClassId);
            return ApiResponse<List<ClassEnrollmentDto>>.ErrorResponse("Error in bulk enrollment", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> UnenrollStudentAsync(Guid classId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Unenrolling student: ClassId={ClassId}, UserId={UserId}", classId, userId);

            var enrollment = await _unitOfWork.ClassEnrollments.GetEnrollmentAsync(classId, userId);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: ClassId={ClassId}, UserId={UserId}", classId, userId);
                return ApiResponse<bool>.ErrorResponse("Enrollment not found");
            }

            _unitOfWork.ClassEnrollments.Remove(enrollment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Student unenrolled successfully: ClassId={ClassId}, UserId={UserId}", classId, userId);
            return ApiResponse<bool>.SuccessResponse(true, "Student unenrolled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unenrolling student: ClassId={ClassId}, UserId={UserId}", classId, userId);
            return ApiResponse<bool>.ErrorResponse("Error unenrolling student", ex.Message);
        }
    }

    public async Task<ApiResponse<int>> GetEnrollmentCountByClassIdAsync(Guid classId)
    {
        try
        {
            _logger.LogInformation("Getting enrollment count for ClassId={ClassId}", classId);

            var enrollments = await _unitOfWork.ClassEnrollments.GetEnrollmentsByClassIdAsync(classId);
            var count = enrollments.Count();

            return ApiResponse<int>.SuccessResponse(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollment count for ClassId={ClassId}", classId);
            return ApiResponse<int>.ErrorResponse("Error getting enrollment count", ex.Message);
        }
    }

    private static IQueryable<ClassEnrollment> ApplySorting(IQueryable<ClassEnrollment> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(e => e.EnrolledAt);

        return sortBy.ToLower() switch
        {
            "enrolledat" => descending ? query.OrderByDescending(e => e.EnrolledAt) : query.OrderBy(e => e.EnrolledAt),
            "studentname" => descending ? query.OrderByDescending(e => e.User.Name) : query.OrderBy(e => e.User.Name),
            "coursecode" => descending ? query.OrderByDescending(e => e.Course.Code) : query.OrderBy(e => e.Course.Code),
            _ => query.OrderByDescending(e => e.EnrolledAt)
        };
    }
}
