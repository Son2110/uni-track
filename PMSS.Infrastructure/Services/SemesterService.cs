using Microsoft.EntityFrameworkCore;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Semester;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<SemesterDto>>> GetAllSemestersAsync(SemesterFilterParams filterParams)
    {
        try
        {
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

            return ApiResponse<PagedResult<SemesterDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<SemesterDto>>.ErrorResponse("Error retrieving semesters", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> GetSemesterByIdAsync(Guid id)
    {
        try
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
                return ApiResponse<SemesterDto>.ErrorResponse("Semester not found");

            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester));
        }
        catch (Exception ex)
        {
            return ApiResponse<SemesterDto>.ErrorResponse("Error retrieving semester", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> CreateSemesterAsync(CreateSemesterDto dto)
    {
        try
        {
            var existingSemester = await _unitOfWork.Semesters.GetByNameAsync(dto.Name);
            if (existingSemester != null)
                return ApiResponse<SemesterDto>.ErrorResponse("Semester with this name already exists");

            if (dto.EndDate <= dto.StartDate)
                return ApiResponse<SemesterDto>.ErrorResponse("End date must be after start date");

            var semester = new Semester
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Semesters.AddAsync(semester);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester), "Semester created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<SemesterDto>.ErrorResponse("Error creating semester", ex.Message);
        }
    }

    public async Task<ApiResponse<SemesterDto>> UpdateSemesterAsync(Guid id, UpdateSemesterDto dto)
    {
        try
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
                return ApiResponse<SemesterDto>.ErrorResponse("Semester not found");

            var existingSemester = await _unitOfWork.Semesters.GetByNameAsync(dto.Name);
            if (existingSemester != null && existingSemester.SemesterId != id)
                return ApiResponse<SemesterDto>.ErrorResponse("Semester with this name already exists");

            if (dto.EndDate <= dto.StartDate)
                return ApiResponse<SemesterDto>.ErrorResponse("End date must be after start date");

            semester.Name = dto.Name;
            semester.StartDate = dto.StartDate;
            semester.EndDate = dto.EndDate;
            semester.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Semesters.Update(semester);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SemesterDto>.SuccessResponse(MapToDto(semester), "Semester updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<SemesterDto>.ErrorResponse("Error updating semester", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteSemesterAsync(Guid id)
    {
        try
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
            if (semester == null)
                return ApiResponse<bool>.ErrorResponse("Semester not found");

            _unitOfWork.Semesters.Remove(semester);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Semester deleted successfully");
        }
        catch (Exception ex)
        {
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
