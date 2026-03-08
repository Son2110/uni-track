using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.AccessRequest;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;
using PMSS.Domain.Enums;

namespace PMSS.Infrastructure.Services;

public class AccessRequestService : IAccessRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccessRequestService> _logger;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public AccessRequestService(IUnitOfWork unitOfWork, ILogger<AccessRequestService> logger, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PagedResult<AccessRequestDto>>> GetAllRequestsAsync(AccessRequestFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all access requests with filters: RequesterId={RequesterId}, ProjectId={ProjectId}, Status={Status}",
                filterParams.RequesterId, filterParams.ProjectId, filterParams.Status);

            var query = (await _unitOfWork.AccessRequests.GetAllAsync()).AsQueryable();

            if (filterParams.RequesterId.HasValue)
                query = query.Where(ar => ar.RequesterId == filterParams.RequesterId.Value);

            if (filterParams.ProjectId.HasValue)
                query = query.Where(ar => ar.ProjectId == filterParams.ProjectId.Value);

            if (filterParams.Status.HasValue)
                query = query.Where(ar => ar.Status == filterParams.Status.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(ar => 
                    ar.Requester.Name.Contains(filterParams.SearchTerm) ||
                    ar.Project.Name.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<AccessRequestDto>>(items);

            var result = new PagedResult<AccessRequestDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<AccessRequestDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access requests");
            return ApiResponse<PagedResult<AccessRequestDto>>.ErrorResponse("Error retrieving access requests", ex.Message);
        }
    }

    public async Task<ApiResponse<AccessRequestDto>> GetRequestByIdAsync(Guid id)
    {
        try
        {
            var request = await _unitOfWork.AccessRequests.GetByIdAsync(id);
            if (request == null)
                return ApiResponse<AccessRequestDto>.ErrorResponse("Access request not found");

            var dto = _mapper.Map<AccessRequestDto>(request);
            return ApiResponse<AccessRequestDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access request {RequestId}", id);
            return ApiResponse<AccessRequestDto>.ErrorResponse("Error retrieving access request", ex.Message);
        }
    }

    public async Task<ApiResponse<AccessRequestDto>> CreateRequestAsync(CreateAccessRequestDto dto)
    {
        try
        {
            _logger.LogInformation("Creating access request: RequesterId={RequesterId}, ProjectId={ProjectId}", 
                dto.RequesterId, dto.ProjectId);

            var requester = await _unitOfWork.Users.GetByIdAsync(dto.RequesterId);
            if (requester == null)
                return ApiResponse<AccessRequestDto>.ErrorResponse("Requester not found");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            if (project == null)
                return ApiResponse<AccessRequestDto>.ErrorResponse("Project not found");

            // Check if request already exists
            var existingRequest = await _unitOfWork.AccessRequests
                .FirstOrDefaultAsync(ar => ar.RequesterId == dto.RequesterId && 
                                          ar.ProjectId == dto.ProjectId && 
                                          ar.Status == AccessRequestStatus.Pending);

            if (existingRequest != null)
                return ApiResponse<AccessRequestDto>.ErrorResponse("A pending access request already exists for this project");

            var accessRequest = new AccessRequest
            {
                RequesterId = dto.RequesterId,
                ProjectId = dto.ProjectId,
                Status = AccessRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _unitOfWork.AccessRequests.AddAsync(accessRequest);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Access request created: RequestId={RequestId}", accessRequest.RequestId);

            var requestDto = _mapper.Map<AccessRequestDto>(accessRequest);
            return ApiResponse<AccessRequestDto>.SuccessResponse(requestDto, "Access request created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access request");
            return ApiResponse<AccessRequestDto>.ErrorResponse("Error creating access request", ex.Message);
        }
    }

    public async Task<ApiResponse<AccessRequestDto>> UpdateRequestStatusAsync(Guid id, UpdateAccessRequestStatusDto dto)
    {
        try
        {
            _logger.LogInformation("Updating access request status: RequestId={RequestId}, Status={Status}", id, dto.Status);

            var request = await _unitOfWork.AccessRequests.GetByIdAsync(id);
            if (request == null)
                return ApiResponse<AccessRequestDto>.ErrorResponse("Access request not found");

            if (request.Status != AccessRequestStatus.Pending)
                return ApiResponse<AccessRequestDto>.ErrorResponse("Cannot update status of a resolved request");

            request.Status = dto.Status;
            request.ResolvedAt = DateTime.UtcNow;

            _unitOfWork.AccessRequests.Update(request);
            await _unitOfWork.SaveChangesAsync();

            // Send notification to the requester
            var isApproved = dto.Status == AccessRequestStatus.Approved;
            var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId);
            await _notificationService.NotifyAccessRequestStatusAsync(
                request.RequesterId, 
                $"project access ({project?.Name})", 
                isApproved);

            _logger.LogInformation("Access request status updated: RequestId={RequestId}, Status={Status}", id, dto.Status);

            var requestDto = _mapper.Map<AccessRequestDto>(request);
            return ApiResponse<AccessRequestDto>.SuccessResponse(requestDto, "Access request status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access request status: RequestId={RequestId}", id);
            return ApiResponse<AccessRequestDto>.ErrorResponse("Error updating access request status", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteRequestAsync(Guid id)
    {
        try
        {
            var request = await _unitOfWork.AccessRequests.GetByIdAsync(id);
            if (request == null)
                return ApiResponse<bool>.ErrorResponse("Access request not found");

            _unitOfWork.AccessRequests.Remove(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Access request deleted: RequestId={RequestId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Access request deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting access request: RequestId={RequestId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting access request", ex.Message);
        }
    }

    private static IQueryable<AccessRequest> ApplySorting(IQueryable<AccessRequest> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(ar => ar.RequestedAt);

        return sortBy.ToLower() switch
        {
            "requestedat" => descending ? query.OrderByDescending(ar => ar.RequestedAt) : query.OrderBy(ar => ar.RequestedAt),
            "status" => descending ? query.OrderByDescending(ar => ar.Status) : query.OrderBy(ar => ar.Status),
            "requestername" => descending ? query.OrderByDescending(ar => ar.Requester.Name) : query.OrderBy(ar => ar.Requester.Name),
            _ => query.OrderByDescending(ar => ar.RequestedAt)
        };
    }
}
