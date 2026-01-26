using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.JiraConfig;

namespace PMSS.Application.Interfaces.Services;

public interface IJiraConfigService
{
    Task<ApiResponse<PagedResult<JiraConfigDto>>> GetAllConfigsAsync(JiraConfigFilterParams filterParams);
    Task<ApiResponse<JiraConfigDto>> GetConfigByIdAsync(Guid id);
    Task<ApiResponse<JiraConfigDto>> GetConfigByProjectIdAsync(Guid projectId);
    Task<ApiResponse<JiraConfigDto>> CreateConfigAsync(CreateJiraConfigDto dto);
    Task<ApiResponse<JiraConfigDto>> UpdateConfigAsync(Guid id, UpdateJiraConfigDto dto);
    Task<ApiResponse<bool>> DeleteConfigAsync(Guid id);
}
