using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.RepoContributor;

namespace PMSS.Application.Interfaces.Services;

public interface IRepoContributorService
{
    Task<ApiResponse<PagedResult<RepoContributorDto>>> GetAllContributorsAsync(RepoContributorFilterParams filterParams);
    Task<ApiResponse<RepoContributorDto>> GetContributorAsync(Guid githubRepoId, string githubUsername);
    Task<ApiResponse<RepoContributorDto>> AddContributorAsync(CreateRepoContributorDto dto);
    Task<ApiResponse<bool>> RemoveContributorAsync(Guid githubRepoId, string githubUsername);
}
