using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.ProjectMember;

namespace PMSS.Application.Interfaces.Services;

public interface IProjectMemberService
{
    Task<ApiResponse<PagedResult<ProjectMemberDto>>> GetAllMembersAsync(ProjectMemberFilterParams filterParams);
    Task<ApiResponse<ProjectMemberDto>> GetMembershipAsync(Guid projectId, Guid userId);
    Task<ApiResponse<ProjectMemberDto>> AddMemberAsync(CreateProjectMemberDto dto);
    Task<ApiResponse<bool>> RemoveMemberAsync(Guid projectId, Guid userId);
}
