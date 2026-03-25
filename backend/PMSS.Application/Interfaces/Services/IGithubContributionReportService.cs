using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubReport;

namespace PMSS.Application.Interfaces.Services;

public interface IGithubContributionReportService
{
    Task<ApiResponse<GithubContributionReportDto>> GenerateAndSaveAsync(
        Guid projectId,
        Guid? generatedByUserId,
        bool usePaidModel = false,
        string? modelOption = null,
        int? recentWeeks = null,
        bool includeMermaidDiagrams = false);

    Task<ApiResponse<GithubContributionReportDto>> GetByIdAsync(Guid reportId);
    Task<ApiResponse<GithubContributionReportDto>> GetLatestByProjectIdAsync(Guid projectId);
    Task<ApiResponse<List<GithubContributionReportSummaryDto>>> GetByProjectIdAsync(Guid projectId, int take = 20);
}