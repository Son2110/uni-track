using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Course;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseService> _logger;
    private readonly IMapper _mapper;

    public CourseService(IUnitOfWork unitOfWork, ILogger<CourseService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<CourseDto>>> GetAllCoursesAsync(CourseFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all courses with filters: Code={Code}, Name={Name}, PageNumber={PageNumber}", 
                filterParams.Code, filterParams.Name, filterParams.PageNumber);

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
                .ToList();

            var itemDtos = _mapper.Map<List<CourseDto>>(items);

            var result = new PagedResult<CourseDto>
            {
                Items = itemDtos,
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

            return ApiResponse<CourseDto>.SuccessResponse(_mapper.Map<CourseDto>(course));
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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            course = await _unitOfWork.Courses.GetByIdAsync(course.CourseId);
            return ApiResponse<CourseDto>.SuccessResponse(_mapper.Map<CourseDto>(course!), "Course created successfully");
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
            course.UpdatedAt = DateTime.Now;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CourseDto>.SuccessResponse(_mapper.Map<CourseDto>(course), "Course updated successfully");
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
