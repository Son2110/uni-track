using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;

namespace PMSS.Application.Interfaces.Services;

public interface IGithubRepoService
{
    Task<ApiResponse<PagedResult<GithubRepoDto>>> GetAllReposAsync(GithubRepoFilterParams filterParams);
    Task<ApiResponse<GithubRepoDto>> GetRepoByIdAsync(Guid id);
    Task<ApiResponse<GithubRepoDto>> CreateRepoAsync(CreateGithubRepoDto dto);
    Task<ApiResponse<GithubRepoDto>> UpdateRepoAsync(Guid id, UpdateGithubRepoDto dto);
    Task<ApiResponse<bool>> DeleteRepoAsync(Guid id);
}
