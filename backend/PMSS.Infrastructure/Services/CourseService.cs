using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Course;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<CourseDto>>> GetAllCoursesAsync(CourseFilterParams filterParams)
    {
        try
        {
            var query = (await _unitOfWork.Courses.GetAllAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterParams.Code))
                query = query.Where(c => c.Code.Contains(filterParams.Code));

            if (!string.IsNullOrWhiteSpace(filterParams.Name))
                query = query.Where(c => c.Name.Contains(filterParams.Name));

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(c => c.Name.Contains(filterParams.SearchTerm) || c.Code.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .Select(c => MapToDto(c))
                .ToList();

            var result = new PagedResult<CourseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<CourseDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<CourseDto>>.ErrorResponse("Error retrieving courses", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseDto>> GetCourseByIdAsync(Guid id)
    {
        try
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                return ApiResponse<CourseDto>.ErrorResponse("Course not found");

            return ApiResponse<CourseDto>.SuccessResponse(MapToDto(course));
        }
        catch (Exception ex)
        {
            return ApiResponse<CourseDto>.ErrorResponse("Error retrieving course", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto)
    {
        try
        {
            var existingCourse = await _unitOfWork.Courses.GetByCodeAsync(dto.Code);
            if (existingCourse != null)
                return ApiResponse<CourseDto>.ErrorResponse("Course with this code already exists");

            var course = new Course
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            course = await _unitOfWork.Courses.GetByIdAsync(course.CourseId);
            return ApiResponse<CourseDto>.SuccessResponse(MapToDto(course!), "Course created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CourseDto>.ErrorResponse("Error creating course", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseDto>> UpdateCourseAsync(Guid id, UpdateCourseDto dto)
    {
        try
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                return ApiResponse<CourseDto>.ErrorResponse("Course not found");

            var existingCourse = await _unitOfWork.Courses.GetByCodeAsync(dto.Code);
            if (existingCourse != null && existingCourse.CourseId != id)
                return ApiResponse<CourseDto>.ErrorResponse("Course with this code already exists");

            course.Code = dto.Code;
            course.Name = dto.Name;
            course.Description = dto.Description;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CourseDto>.SuccessResponse(MapToDto(course), "Course updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CourseDto>.ErrorResponse("Error updating course", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteCourseAsync(Guid id)
    {
        try
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                return ApiResponse<bool>.ErrorResponse("Course not found");

            _unitOfWork.Courses.Remove(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Course deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse("Error deleting course", ex.Message);
        }
    }

    private static CourseDto MapToDto(Course course)
    {
        return new CourseDto
        {
            CourseId = course.CourseId,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }

    private static IQueryable<Course> ApplySorting(IQueryable<Course> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderBy(c => c.Name);

        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "code" => descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Name)
        };
    }
}
