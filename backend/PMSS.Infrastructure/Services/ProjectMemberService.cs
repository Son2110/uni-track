using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.ProjectMember;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectMemberService> _logger;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ProjectMemberService(IUnitOfWork unitOfWork, ILogger<ProjectMemberService> logger, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PagedResult<ProjectMemberDto>>> GetAllMembersAsync(ProjectMemberFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all project members with filters: ProjectId={ProjectId}, UserId={UserId}", 
                filterParams.ProjectId, filterParams.UserId);

            var query = (await _unitOfWork.ProjectMembers.GetAllAsync()).AsQueryable();

            // Apply filters
            if (filterParams.ProjectId.HasValue)
            {
                query = query.Where(pm => pm.ProjectId == filterParams.ProjectId.Value);
            }

            if (filterParams.UserId.HasValue)
            {
                query = query.Where(pm => pm.UserId == filterParams.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                query = query.Where(pm =>
                    pm.User.Name.Contains(filterParams.SearchTerm) ||
                    pm.User.Email.Contains(filterParams.SearchTerm) ||
                    pm.Project.Name.Contains(filterParams.SearchTerm));
            }

            var totalCount = query.Count();

            // Apply sorting
            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            // Apply pagination
            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<ProjectMemberDto>>(items);

            var result = new PagedResult<ProjectMemberDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<ProjectMemberDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project members");
            return ApiResponse<PagedResult<ProjectMemberDto>>.ErrorResponse("Error retrieving project members", ex.Message);
        }
    }

    public async Task<ApiResponse<ProjectMemberDto>> GetMembershipAsync(Guid projectId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting membership for ProjectId={ProjectId}, UserId={UserId}", projectId, userId);

            var membership = await _unitOfWork.ProjectMembers.GetMembershipAsync(projectId, userId);
            
            if (membership == null)
            {
                return ApiResponse<ProjectMemberDto>.ErrorResponse("Membership not found");
            }

            return ApiResponse<ProjectMemberDto>.SuccessResponse(_mapper.Map<ProjectMemberDto>(membership));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving membership");
            return ApiResponse<ProjectMemberDto>.ErrorResponse("Error retrieving membership", ex.Message);
        }
    }

    public async Task<ApiResponse<ProjectMemberDto>> AddMemberAsync(CreateProjectMemberDto dto)
    {
        try
        {
            _logger.LogInformation("Adding member: ProjectId={ProjectId}, UserId={UserId}", dto.ProjectId, dto.UserId);

            // Validate project exists
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            if (project == null)
            {
                return ApiResponse<ProjectMemberDto>.ErrorResponse("Project not found");
            }

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return ApiResponse<ProjectMemberDto>.ErrorResponse("User not found");
            }

            // Check if membership already exists
            var existingMembership = await _unitOfWork.ProjectMembers.GetMembershipAsync(dto.ProjectId, dto.UserId);
            if (existingMembership != null)
            {
                return ApiResponse<ProjectMemberDto>.ErrorResponse("User is already a member of this project");
            }

            // Create new membership
            var projectMember = new ProjectMember
            {
                ProjectId = dto.ProjectId,
                UserId = dto.UserId,
                JoinedAt = DateTime.Now
            };

            await _unitOfWork.ProjectMembers.AddAsync(projectMember);
            await _unitOfWork.SaveChangesAsync();

            // Send notification to the added member
            await _notificationService.NotifyProjectMemberAddedAsync(dto.UserId, project.Name);

            // Retrieve the full membership data
            var membership = await _unitOfWork.ProjectMembers.GetMembershipAsync(dto.ProjectId, dto.UserId);

            _logger.LogInformation("Member added successfully: ProjectId={ProjectId}, UserId={UserId}", dto.ProjectId, dto.UserId);
            return ApiResponse<ProjectMemberDto>.SuccessResponse(_mapper.Map<ProjectMemberDto>(membership!), "Member added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member");
            return ApiResponse<ProjectMemberDto>.ErrorResponse("Error adding member", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveMemberAsync(Guid projectId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Removing member: ProjectId={ProjectId}, UserId={UserId}", projectId, userId);

            var membership = await _unitOfWork.ProjectMembers.GetMembershipAsync(projectId, userId);
            
            if (membership == null)
            {
                return ApiResponse<bool>.ErrorResponse("Membership not found");
            }

            _unitOfWork.ProjectMembers.Remove(membership);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Member removed successfully: ProjectId={ProjectId}, UserId={UserId}", projectId, userId);
            return ApiResponse<bool>.SuccessResponse(true, "Member removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member");
            return ApiResponse<bool>.ErrorResponse("Error removing member", ex.Message);
        }
    }

    private static IQueryable<ProjectMember> ApplySorting(IQueryable<ProjectMember> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(pm => pm.JoinedAt);

        return sortBy.ToLower() switch
        {
            "joinedat" => descending ? query.OrderByDescending(pm => pm.JoinedAt) : query.OrderBy(pm => pm.JoinedAt),
            "username" => descending ? query.OrderByDescending(pm => pm.User.Name) : query.OrderBy(pm => pm.User.Name),
            "projectname" => descending ? query.OrderByDescending(pm => pm.Project.Name) : query.OrderBy(pm => pm.Project.Name),
            _ => query.OrderByDescending(pm => pm.JoinedAt)
        };
    }
}
