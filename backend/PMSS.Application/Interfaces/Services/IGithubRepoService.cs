using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;

namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service for managing GitHub repositories. All project members have equal permissions to manage repositories.
/// </summary>
public interface IGithubRepoService
{
    Task<ApiResponse<PagedResult<GithubRepoDto>>> GetAllReposAsync(GithubRepoFilterParams filterParams);
    Task<ApiResponse<GithubRepoDto>> GetRepoByIdAsync(Guid id);
    Task<ApiResponse<GithubRepoDto>> CreateRepoAsync(CreateGithubRepoDto dto, Guid createdByUserId);
    Task<ApiResponse<GithubRepoDto>> UpdateRepoAsync(Guid id, UpdateGithubRepoDto dto, Guid updatedByUserId);
    Task<ApiResponse<bool>> DeleteRepoAsync(Guid id, Guid deletedByUserId);
    Task<ApiResponse<CourseGithubReposDto>> GetReposByCourseIdAsync(Guid courseId);
    Task<ApiResponse<List<GithubRepoDto>>> GetReposByUserIdAsync(Guid userId);
    Task<ApiResponse<GithubRepoDto>> AddContributorToRepoAsync(Guid repoId, Guid userId, Guid addedByUserId);
    Task<ApiResponse<bool>> RemoveContributorFromRepoAsync(Guid repoId, Guid userId, Guid removedByUserId);
    Task<ApiResponse<bool>> CanUserManageRepoAsync(Guid repoId, Guid userId);
    Task<ApiResponse<ProjectGithubContributionDto>> GetGithubContributionsAsync(Guid projectId);
}
