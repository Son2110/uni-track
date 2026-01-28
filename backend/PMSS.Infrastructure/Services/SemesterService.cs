using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Semester;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SemesterService> _logger;

    public SemesterService(IUnitOfWork unitOfWork, ILogger<SemesterService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<SemesterDto>>> GetAllSemestersAsync(SemesterFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all semesters with filters: PageNumber={PageNumber}, PageSize={PageSize}", 
                filterParams.PageNumber, filterParams.PageSize);

            var query = (await _unitOfWork.Semesters.GetAllAsync()).AsQueryable();

            if (filterParams.StartDateFrom.HasValue)
                query = query.Where(s => s.StartDate >= filterParams.StartDateFrom.Value);

            if (filterParams.StartDateTo.HasValue)
                query = query.Where(s => s.StartDate <= filterParams.StartDateTo.Value);

            if (filterParams.EndDateFrom.HasValue)
                query = query.Where(s => s.EndDate >= filterParams.EndDateFrom.Value);

            if (filterParams.EndDateTo.HasValue)
                query = query.Where(s => s.EndDate <= filterParams.EndDateTo.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(s => s.Name.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .Select(s => MapToDto(s))
                .ToList();

            var result = new PagedResult<SemesterDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            _logger.LogInformation("Successfully retrieved {Count} semesters", items.Count);
            return ApiResponse<PagedResult<SemesterDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving semesters");
            return ApiResponse<PagedResult<SemesterDto>>.ErrorResponse("Error retrieving semesters", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> GetSemesterByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting semester by id: {SemesterId}", id);
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                _logger.LogWarning("Semester not found: {SemesterId}", id);
                return ApiResponse<SemesterDto>.ErrorResponse("Semester not found");
            }

            _logger.LogInformation("Successfully retrieved semester: {SemesterId}", id);
            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving semester: {SemesterId}", id);
            return ApiResponse<SemesterDto>.ErrorResponse("Error retrieving semester", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> CreateSemesterAsync(CreateSemesterDto dto)
    {
        try
        {
            _logger.LogInformation("Creating semester: {SemesterName}", dto.Name);
            var existingSemester = await _unitOfWork.Semesters.GetByNameAsync(dto.Name);
            if (existingSemester != null)
            {
                _logger.LogWarning("Semester with name already exists: {SemesterName}", dto.Name);
                return ApiResponse<SemesterDto>.ErrorResponse("Semester with this name already exists");
            }

            if (dto.EndDate <= dto.StartDate)
            {
                _logger.LogWarning("Invalid date range for semester: {SemesterName}", dto.Name);
                return ApiResponse<SemesterDto>.ErrorResponse("End date must be after start date");
            }

            var semester = new Semester
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Semesters.AddAsync(semester);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created semester: {SemesterId}", semester.SemesterId);
            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester), "Semester created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating semester: {SemesterName}", dto.Name);
            return ApiResponse<SemesterDto>.ErrorResponse("Error creating semester", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> UpdateSemesterAsync(Guid id, UpdateSemesterDto dto)
    {
        try
        {
            _logger.LogInformation("Updating semester: {SemesterId}", id);
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                _logger.LogWarning("Semester not found for update: {SemesterId}", id);
                return ApiResponse<SemesterDto>.ErrorResponse("Semester not found");
            }

            var existingSemester = await _unitOfWork.Semesters.GetByNameAsync(dto.Name);
            if (existingSemester != null && existingSemester.SemesterId != id)
            {
                _logger.LogWarning("Semester with name already exists: {SemesterName}", dto.Name);
                return ApiResponse<SemesterDto>.ErrorResponse("Semester with this name already exists");
            }

            if (dto.EndDate <= dto.StartDate)
            {
                _logger.LogWarning("Invalid date range for semester update: {SemesterId}", id);
                return ApiResponse<SemesterDto>.ErrorResponse("End date must be after start date");
            }

            semester.Name = dto.Name;
            semester.StartDate = dto.StartDate;
            semester.EndDate = dto.EndDate;
            semester.UpdatedAt = DateTime.Now;

            _unitOfWork.Semesters.Update(semester);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated semester: {SemesterId}", id);
            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester), "Semester updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating semester: {SemesterId}", id);
            return ApiResponse<SemesterDto>.ErrorResponse("Error updating semester", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteSemesterAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting semester: {SemesterId}", id);
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
            {
                _logger.LogWarning("Semester not found for deletion: {SemesterId}", id);
                return ApiResponse<bool>.ErrorResponse("Semester not found");
            }

            _unitOfWork.Semesters.Remove(semester);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted semester: {SemesterId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Semester deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting semester: {SemesterId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting semester", ex.Message);
        }
    }

    private static SemesterDto MapToDto(Semester semester)
    {
        return new SemesterDto
        {
            SemesterId = semester.SemesterId,
            Name = semester.Name,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            CreatedAt = semester.CreatedAt,
            UpdatedAt = semester.UpdatedAt
        };
    }

    private static IQueryable<Semester> ApplySorting(IQueryable<Semester> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(s => s.CreatedAt);

        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "startdate" => descending ? query.OrderByDescending(s => s.StartDate) : query.OrderBy(s => s.StartDate),
            "enddate" => descending ? query.OrderByDescending(s => s.EndDate) : query.OrderBy(s => s.EndDate),
            "createdat" => descending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };
    }
}
