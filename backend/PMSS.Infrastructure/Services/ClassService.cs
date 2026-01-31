using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Class;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class ClassService : IClassService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClassService> _logger;
    private readonly IMapper _mapper;

    public ClassService(IUnitOfWork unitOfWork, ILogger<ClassService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<ClassDto>>> GetAllClassesAsync(ClassFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all classes with filters: SemesterId={SemesterId}, CourseId={CourseId}, TeacherId={TeacherId}, PageNumber={PageNumber}",
                filterParams.SemesterId, filterParams.CourseId, filterParams.TeacherId, filterParams.PageNumber);

            var query = (await _unitOfWork.Classes.GetAllAsync()).AsQueryable();

            if (filterParams.SemesterId.HasValue)
                query = query.Where(c => c.SemesterId == filterParams.SemesterId.Value);

            if (filterParams.CourseId.HasValue)
                query = query.Where(c => c.CourseId == filterParams.CourseId.Value);

            if (filterParams.TeacherId.HasValue)
                query = query.Where(c => c.TeacherId == filterParams.TeacherId.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(c => c.ClassCode.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<ClassDto>>(items);

            var result = new PagedResult<ClassDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<ClassDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classes");
            return ApiResponse<PagedResult<ClassDto>>.ErrorResponse("Error retrieving classes", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> GetClassByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting class by id: {ClassId}", id);

            var classEntity = await _unitOfWork.Classes.GetByIdWithDetailsAsync(id);
            if (classEntity == null)
            {
                _logger.LogWarning("Class not found: {ClassId}", id);
                return ApiResponse<ClassDto>.ErrorResponse("Class not found");
            }

            return ApiResponse<ClassDto>.SuccessResponse(_mapper.Map<ClassDto>(classEntity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class: {ClassId}", id);
            return ApiResponse<ClassDto>.ErrorResponse("Error retrieving class", ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesByTeacherIdAsync(Guid teacherId)
    {
        try
        {
            _logger.LogInformation("Getting classes by teacher id: {TeacherId}", teacherId);

            var classes = await _unitOfWork.Classes.GetClassesByTeacherIdAsync(teacherId);
            var classDtos = _mapper.Map<List<ClassDto>>(classes);

            return ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classes for teacher: {TeacherId}", teacherId);
            return ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Error retrieving classes", ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesBySemesterIdAsync(Guid semesterId)
    {
        try
        {
            _logger.LogInformation("Getting classes by semester id: {SemesterId}", semesterId);

            var classes = await _unitOfWork.Classes.GetClassesBySemesterIdAsync(semesterId);
            var classDtos = _mapper.Map<List<ClassDto>>(classes);

            return ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classes for semester: {SemesterId}", semesterId);
            return ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Error retrieving classes", ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<ClassDto>>> GetClassesByCourseIdAsync(Guid courseId)
    {
        try
        {
            _logger.LogInformation("Getting classes by course id: {CourseId}", courseId);

            var classes = await _unitOfWork.Classes.GetClassesByCourseIdAsync(courseId);
            var classDtos = _mapper.Map<List<ClassDto>>(classes);

            return ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classes for course: {CourseId}", courseId);
            return ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Error retrieving classes", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassDto dto)
    {
        try
        {
            _logger.LogInformation("Creating class for Semester: {SemesterId}, Course: {CourseId}, ClassCode: {ClassCode}",
                dto.SemesterId, dto.CourseId, dto.ClassCode);

            // Validate semester exists
            var semester = await _unitOfWork.Semesters.GetByIdAsync(dto.SemesterId);
            if (semester == null)
            {
                _logger.LogWarning("Semester not found: {SemesterId}", dto.SemesterId);
                return ApiResponse<ClassDto>.ErrorResponse("Semester not found");
            }

            // Validate course exists
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", dto.CourseId);
                return ApiResponse<ClassDto>.ErrorResponse("Course not found");
            }

            // Validate teacher exists
            var teacher = await _unitOfWork.Users.GetByIdAsync(dto.TeacherId);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found: {TeacherId}", dto.TeacherId);
                return ApiResponse<ClassDto>.ErrorResponse("Teacher not found");
            }

            // Check if class already exists
            var existingClass = await _unitOfWork.Classes.GetClassBySemesterCourseAndSectionAsync(
                dto.SemesterId, dto.CourseId, dto.ClassCode);
            if (existingClass != null)
            {
                _logger.LogWarning("Class already exists for Semester: {SemesterId}, Course: {CourseId}, ClassCode: {ClassCode}",
                    dto.SemesterId, dto.CourseId, dto.ClassCode);
                return ApiResponse<ClassDto>.ErrorResponse("Class with this semester, course, and class code already exists");
            }

            var classEntity = new Class
            {
                SemesterId = dto.SemesterId,
                CourseId = dto.CourseId,
                ClassCode = dto.ClassCode,
                TeacherId = dto.TeacherId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Classes.AddAsync(classEntity);
            await _unitOfWork.SaveChangesAsync();

            classEntity = await _unitOfWork.Classes.GetByIdWithDetailsAsync(classEntity.ClassId);
            _logger.LogInformation("Class created successfully: {ClassId}", classEntity!.ClassId);

            return ApiResponse<ClassDto>.SuccessResponse(_mapper.Map<ClassDto>(classEntity), "Class created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class");
            return ApiResponse<ClassDto>.ErrorResponse("Error creating class", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassDto dto)
    {
        try
        {
            _logger.LogInformation("Updating class: {ClassId}", id);

            var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
            if (classEntity == null)
            {
                _logger.LogWarning("Class not found: {ClassId}", id);
                return ApiResponse<ClassDto>.ErrorResponse("Class not found");
            }

            // Validate teacher exists
            var teacher = await _unitOfWork.Users.GetByIdAsync(dto.TeacherId);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found: {TeacherId}", dto.TeacherId);
                return ApiResponse<ClassDto>.ErrorResponse("Teacher not found");
            }

            classEntity.ClassCode = dto.ClassCode;
            classEntity.TeacherId = dto.TeacherId;
            classEntity.UpdatedAt = DateTime.Now;

            _unitOfWork.Classes.Update(classEntity);
            await _unitOfWork.SaveChangesAsync();

            classEntity = await _unitOfWork.Classes.GetByIdWithDetailsAsync(id);
            _logger.LogInformation("Class updated successfully: {ClassId}", id);

            return ApiResponse<ClassDto>.SuccessResponse(_mapper.Map<ClassDto>(classEntity!), "Class updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class: {ClassId}", id);
            return ApiResponse<ClassDto>.ErrorResponse("Error updating class", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteClassAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting class: {ClassId}", id);

            var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
            if (classEntity == null)
            {
                _logger.LogWarning("Class not found: {ClassId}", id);
                return ApiResponse<bool>.ErrorResponse("Class not found");
            }

            _unitOfWork.Classes.Remove(classEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Class deleted successfully: {ClassId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Class deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class: {ClassId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting class", ex.Message);
        }
    }

    private static IQueryable<Class> ApplySorting(IQueryable<Class> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderBy(c => c.ClassCode);

        return sortBy.ToLower() switch
        {
            "classcode" => descending ? query.OrderByDescending(c => c.ClassCode) : query.OrderBy(c => c.ClassCode),
            "section" => descending ? query.OrderByDescending(c => c.ClassCode) : query.OrderBy(c => c.ClassCode),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "updatedat" => descending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            _ => query.OrderBy(c => c.ClassCode)
        };
    }
}
