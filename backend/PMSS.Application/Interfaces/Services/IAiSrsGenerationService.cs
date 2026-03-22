using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

public interface IAiSrsGenerationService
{
    Task<ApiResponse<byte[]>> GenerateSrsDocxAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null);
    Task<ApiResponse<string>> GenerateSrsMarkdownAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null);
    Task<ApiResponse<string>> GenerateGithubReportMarkdownAsync(Guid projectId, bool usePaidModel = false, string? modelOption = null);
}
