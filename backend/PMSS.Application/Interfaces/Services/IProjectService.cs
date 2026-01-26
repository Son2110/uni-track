using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.DTOs.Project;

namespace PMSS.Application.Interfaces.Services;

public interface IProjectService
{
    Task<ApiResponse<PagedResult<ProjectDto>>> GetAllProjectsAsync(ProjectFilterParams filterParams);
    Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id);
    Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto);
    Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectDto dto);
    Task<ApiResponse<bool>> DeleteProjectAsync(Guid id);
    Task<ApiResponse<ProjectGithubContributionDto>> GetProjectGithubContributionsAsync(Guid projectId);
}
