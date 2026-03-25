using System.ClientModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubReport;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Configuration;

namespace PMSS.Infrastructure.Services;

public class GithubContributionReportService : IGithubContributionReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GitHubModelsSettings _modelsSettings;
    private readonly OpenAISettings _openAISettings;
    private readonly ILogger<GithubContributionReportService> _logger;

    public GithubContributionReportService(
        IUnitOfWork unitOfWork,
        IOptions<GitHubModelsSettings> modelsSettings,
        IOptions<OpenAISettings> openAISettings,
        ILogger<GithubContributionReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _modelsSettings = modelsSettings.Value;
        _openAISettings = openAISettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<GithubContributionReportDto>> GenerateAndSaveAsync(
        Guid projectId,
        Guid? generatedByUserId,
        bool usePaidModel = false,
        string? modelOption = null,
        int? recentWeeks = null,
        bool includeMermaidDiagrams = false)
    {
        try
        {
            if (usePaidModel)
            {
                if (string.IsNullOrWhiteSpace(_openAISettings.ApiKey))
                    return ApiResponse<GithubContributionReportDto>.ErrorResponse("OpenAI API key is not configured");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_modelsSettings.GitHubToken))
                    return ApiResponse<GithubContributionReportDto>.ErrorResponse("GitHub Models API token is not configured");
            }

            var repos = (await _unitOfWork.GithubRepos.GetReposByProjectIdWithSemesterAsync(projectId)).ToList();
            if (!repos.Any())
                return ApiResponse<GithubContributionReportDto>.ErrorResponse("No GitHub repositories found for this project");

            var project = repos[0].Project;
            var semester = project.Class.Semester;

            var now = DateTime.UtcNow.Date;
            var periodStart = semester.StartDate.Date;
            var periodEnd = semester.EndDate.Date < now ? semester.EndDate.Date : now;

            if (recentWeeks.HasValue && recentWeeks.Value > 0)
            {
                var weeksStart = now.AddDays(-7 * recentWeeks.Value);
                if (weeksStart > periodStart)
                    periodStart = weeksStart;
            }

            var repoIds = repos.Select(r => r.GithubRepoId).ToList();
            var weeklyContributions = (await _unitOfWork.WeeklyContributions
                .GetByRepoIdsAndDateRangeAsync(repoIds, periodStart, periodEnd))
                .ToList();

            var weeklyContributionIds = weeklyContributions.Select(x => x.WeeklyContributionId).ToList();
            var userContributions = weeklyContributionIds.Count == 0
                ? []
                : (await _unitOfWork.UserWeeklyContributions.GetByWeeklyContributionIdsAsync(weeklyContributionIds)).ToList();

            var contributorMetrics = userContributions
                .GroupBy(x => x.GithubUsername, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ContributorMetric
                {
                    GithubUsername = g.First().GithubUsername,
                    UserId = g.First().UserId,
                    UserName = g.First().User?.Name,
                    TotalCommits = g.Sum(x => x.Commits),
                    TotalAdditions = g.Sum(x => x.Additions),
                    TotalDeletions = g.Sum(x => x.Deletions)
                })
                .OrderByDescending(x => x.TotalCommits)
                .ThenByDescending(x => x.TotalAdditions)
                .ToList();

            var totalCommits = contributorMetrics.Sum(x => x.TotalCommits);
            var totalAdditions = contributorMetrics.Sum(x => x.TotalAdditions);
            var totalDeletions = contributorMetrics.Sum(x => x.TotalDeletions);
            var activeContributorCount = contributorMetrics.Count(x => x.TotalCommits > 0 || x.TotalAdditions > 0 || x.TotalDeletions > 0);
            var contributorCount = contributorMetrics.Count;

            var weeklyTrend = weeklyContributions
                .GroupBy(x => x.WeekStart.Date)
                .Select(g => new WeeklyMetric
                {
                    WeekStart = g.Key,
                    Commits = g.Sum(x => x.TotalCommits),
                    Additions = g.Sum(x => x.TotalAdditions),
                    Deletions = g.Sum(x => x.TotalDeletions)
                })
                .OrderBy(x => x.WeekStart)
                .ToList();

            var totalWeeks = Math.Max(1, (int)Math.Ceiling((periodEnd - periodStart).TotalDays / 7.0));
            var activeWeeks = weeklyTrend.Count(x => x.Commits > 0 || x.Additions > 0 || x.Deletions > 0);
            var consistencyScore = (double)activeWeeks / totalWeeks;

            var topContributorCommits = contributorMetrics.FirstOrDefault()?.TotalCommits ?? 0;
            var dominanceScore = totalCommits == 0 ? 0 : (double)topContributorCommits / totalCommits;

            var busFactor = CalculateBusFactor(contributorMetrics, totalCommits);
            var riskFlags = BuildRiskFlags(totalCommits, activeContributorCount, consistencyScore, dominanceScore, busFactor);

            var insightPayload = new
            {
                periodStart,
                periodEnd,
                totalCommits,
                totalAdditions,
                totalDeletions,
                contributorCount,
                activeContributorCount,
                activeWeeks,
                totalWeeks,
                consistencyScore = Math.Round(consistencyScore, 3),
                dominanceScore = Math.Round(dominanceScore, 3),
                busFactor,
                riskFlags,
                topContributors = contributorMetrics.Take(5).Select(x => new
                {
                    x.GithubUsername,
                    x.UserName,
                    x.TotalCommits,
                    x.TotalAdditions,
                    x.TotalDeletions
                }),
                weeklyTrend = weeklyTrend.Select(x => new
                {
                    x.WeekStart,
                    x.Commits,
                    x.Additions,
                    x.Deletions
                })
            };

            var insightsJson = JsonSerializer.Serialize(insightPayload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var markdown = await GenerateMarkdownAsync(
                project.Name,
                repos,
                periodStart,
                periodEnd,
                insightPayload,
                usePaidModel,
                modelOption,
                includeMermaidDiagrams);

            var executiveSummary = BuildExecutiveSummary(totalCommits, activeContributorCount, contributorCount, dominanceScore, consistencyScore, riskFlags);

            var selectedModel = usePaidModel
                ? ResolvePaidModelName(_openAISettings.DefaultGithubReportModelOption, modelOption)
                : _modelsSettings.ModelName;

            var entity = new GithubContributionReport
            {
                GithubContributionReportId = Guid.NewGuid(),
                ProjectId = projectId,
                GeneratedByUserId = generatedByUserId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                TotalCommits = totalCommits,
                TotalAdditions = totalAdditions,
                TotalDeletions = totalDeletions,
                ContributorCount = contributorCount,
                ActiveContributorCount = activeContributorCount,
                ModelProvider = usePaidModel ? "openai" : "github-models",
                ModelName = selectedModel,
                ExecutiveSummary = executiveSummary,
                InsightsJson = insightsJson,
                MarkdownContent = markdown,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GithubContributionReports.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<GithubContributionReportDto>.SuccessResponse(MapToDto(entity), "GitHub contribution report generated and saved successfully");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "AI API error while generating GitHub contribution report for project {ProjectId}", projectId);
            return ApiResponse<GithubContributionReportDto>.ErrorResponse(GetFriendlyAIErrorMessage(ex, usePaidModel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating GitHub contribution report for project {ProjectId}", projectId);
            return ApiResponse<GithubContributionReportDto>.ErrorResponse("Error generating GitHub contribution report", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubContributionReportDto>> GetByIdAsync(Guid reportId)
    {
        try
        {
            var report = await _unitOfWork.GithubContributionReports.GetByIdWithProjectAsync(reportId);
            if (report == null)
                return ApiResponse<GithubContributionReportDto>.ErrorResponse("GitHub contribution report not found");

            return ApiResponse<GithubContributionReportDto>.SuccessResponse(MapToDto(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub contribution report {ReportId}", reportId);
            return ApiResponse<GithubContributionReportDto>.ErrorResponse("Error retrieving GitHub contribution report", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubContributionReportDto>> GetLatestByProjectIdAsync(Guid projectId)
    {
        try
        {
            var report = await _unitOfWork.GithubContributionReports.GetLatestByProjectIdAsync(projectId);
            if (report == null)
                return ApiResponse<GithubContributionReportDto>.ErrorResponse("No GitHub contribution report found for this project");

            return ApiResponse<GithubContributionReportDto>.SuccessResponse(MapToDto(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest GitHub contribution report for project {ProjectId}", projectId);
            return ApiResponse<GithubContributionReportDto>.ErrorResponse("Error retrieving latest GitHub contribution report", ex.Message);
        }
    }

    public async Task<ApiResponse<List<GithubContributionReportSummaryDto>>> GetByProjectIdAsync(Guid projectId, int take = 20)
    {
        try
        {
            var reports = await _unitOfWork.GithubContributionReports.GetByProjectIdAsync(projectId, take);
            var result = reports.Select(MapToSummary).ToList();
            return ApiResponse<List<GithubContributionReportSummaryDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub contribution reports for project {ProjectId}", projectId);
            return ApiResponse<List<GithubContributionReportSummaryDto>>.ErrorResponse("Error retrieving GitHub contribution reports", ex.Message);
        }
    }

    private async Task<string> GenerateMarkdownAsync(
        string projectName,
        List<GithubRepo> repos,
        DateTime periodStart,
        DateTime periodEnd,
        object insightPayload,
        bool usePaidModel,
        string? modelOption,
        bool includeMermaidDiagrams)
    {
        var client = usePaidModel
            ? new OpenAIClient(
                new ApiKeyCredential(_openAISettings.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(_openAISettings.Endpoint),
                    NetworkTimeout = TimeSpan.FromSeconds(_openAISettings.NetworkTimeoutInSeconds)
                })
            : new OpenAIClient(
                new ApiKeyCredential(_modelsSettings.GitHubToken),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(_modelsSettings.Endpoint)
                });

        var selectedModel = usePaidModel
            ? ResolvePaidModelName(_openAISettings.DefaultGithubReportModelOption, modelOption)
            : _modelsSettings.ModelName;

        var chatClient = client.GetChatClient(selectedModel);

        var repoList = string.Join("\n", repos.Select(r => $"- {r.RepoOwnerName}/{r.RepoName}"));
        var insightJson = JsonSerializer.Serialize(insightPayload, new JsonSerializerOptions { WriteIndented = true });

        var mermaidInstruction = includeMermaidDiagrams
            ? "If trend/flow can be visualized, include 1-2 Mermaid code blocks using stable Mermaid syntaxes only (prefer flowchart or pie). Do not use xychart/xychart-beta. Output valid Mermaid that can be pasted directly into Mermaid Live Editor."
            : "Do not include Mermaid code blocks unless explicitly required by the user.";

        var systemPrompt = $"""
            You are a senior software engineering analytics consultant.
            Write a concise, evidence-based GitHub contribution report in Markdown.

            Requirements:
            - Use ONLY the numbers and facts provided in the input.
            - Each insight must include supporting evidence (specific metrics/trends).
            - Avoid vague statements; provide specific actions for team lead and members.
            - If data is missing, explicitly say: N/A (data unavailable).
            - {mermaidInstruction}

            Output sections (exact order):
            1) # GitHub Contribution Report
            2) ## Executive Summary
            3) ## Key Metrics
            4) ## Contributor Analysis
            5) ## Weekly Trend Analysis
            6) ## Risk Signals
            7) ## Action Plan (Next 2 Weeks)
            """;

        var userPrompt = $"""
            Project: {projectName}
            Repositories:
            {repoList}
            Reporting Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}

            Structured Metrics:
            {insightJson}

            Generate the report now.
            """;

        var completion = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions
            {
                Temperature = 0.2f,
                MaxOutputTokenCount = usePaidModel ? _openAISettings.MaxOutputTokens : 4096
            });

        return ExtractCompletionText(completion.Value);
    }

    private static int CalculateBusFactor(List<ContributorMetric> contributors, int totalCommits)
    {
        if (totalCommits <= 0 || contributors.Count == 0)
            return 0;

        var threshold = totalCommits * 0.5;
        var running = 0;
        var count = 0;

        foreach (var contributor in contributors.OrderByDescending(x => x.TotalCommits))
        {
            running += contributor.TotalCommits;
            count++;
            if (running >= threshold)
                return count;
        }

        return contributors.Count;
    }

    private static List<string> BuildRiskFlags(int totalCommits, int activeContributorCount, double consistencyScore, double dominanceScore, int busFactor)
    {
        var risks = new List<string>();

        if (totalCommits == 0)
            risks.Add("No coding activity detected in selected period");
        if (activeContributorCount <= 1 && totalCommits > 0)
            risks.Add("Single active contributor dependency");
        if (dominanceScore >= 0.6)
            risks.Add("Workload concentration is high (top contributor dominates)");
        if (consistencyScore < 0.4)
            risks.Add("Delivery cadence is inconsistent across weeks");
        if (busFactor <= 1 && totalCommits > 0)
            risks.Add("Low bus factor risk");

        if (risks.Count == 0)
            risks.Add("No critical risk signal detected from current metrics");

        return risks;
    }

    private static string BuildExecutiveSummary(
        int totalCommits,
        int activeContributorCount,
        int contributorCount,
        double dominanceScore,
        double consistencyScore,
        List<string> riskFlags)
    {
        var summary = $"{totalCommits} commits from {activeContributorCount}/{contributorCount} active contributors. ";
        summary += $"Dominance score: {dominanceScore:P0}, consistency score: {consistencyScore:P0}. ";
        summary += $"Top risk: {riskFlags.FirstOrDefault() ?? "N/A"}.";
        return summary.Length <= 2000 ? summary : summary[..2000];
    }

    private string ResolvePaidModelName(string defaultOption, string? modelOption)
    {
        var option = string.IsNullOrWhiteSpace(modelOption)
            ? defaultOption
            : modelOption.Trim().ToLowerInvariant();

        return option switch
        {
            "fast" => _openAISettings.FastModelName,
            "balanced" => _openAISettings.BalancedModelName,
            "quality" => _openAISettings.QualityModelName,
            _ => defaultOption.ToLowerInvariant() switch
            {
                "fast" => _openAISettings.FastModelName,
                "balanced" => _openAISettings.BalancedModelName,
                _ => _openAISettings.QualityModelName
            }
        };
    }

    private static string GetFriendlyAIErrorMessage(ClientResultException ex, bool usePaidModel)
    {
        var message = ex.Message;
        var statusCode = ex.Status;
        var modelLabel = usePaidModel ? "OpenAI (paid)" : "GitHub Models (free)";

        return statusCode switch
        {
            429 => $"The {modelLabel} API quota has been exceeded. Please check billing and plan details.",
            401 => $"The {modelLabel} API key is invalid or has been revoked.",
            403 => $"Access denied by the {modelLabel} API.",
            413 => $"The request is too large for the {modelLabel} API.",
            500 or 502 or 503 => $"The {modelLabel} API is temporarily unavailable. Please try again later.",
            _ => $"The {modelLabel} API returned an error (HTTP {statusCode}): {message}"
        };
    }

    private static string ExtractCompletionText(ChatCompletion completion)
    {
        if (completion.Content is not { Count: > 0 })
            throw new InvalidOperationException($"The AI model returned no content. Finish reason: {completion.FinishReason}");

        var text = completion.Content[0].Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"The AI model returned empty content. Finish reason: {completion.FinishReason}");

        return text;
    }

    private static GithubContributionReportDto MapToDto(GithubContributionReport entity)
    {
        return new GithubContributionReportDto
        {
            ReportId = entity.GithubContributionReportId,
            ProjectId = entity.ProjectId,
            PeriodStart = entity.PeriodStart,
            PeriodEnd = entity.PeriodEnd,
            TotalCommits = entity.TotalCommits,
            ContributorCount = entity.ContributorCount,
            ActiveContributorCount = entity.ActiveContributorCount,
            ExecutiveSummary = entity.ExecutiveSummary,
            ModelProvider = entity.ModelProvider,
            ModelName = entity.ModelName,
            CreatedAt = entity.CreatedAt,
            InsightsJson = entity.InsightsJson,
            MarkdownContent = entity.MarkdownContent
        };
    }

    private static GithubContributionReportSummaryDto MapToSummary(GithubContributionReport entity)
    {
        return new GithubContributionReportSummaryDto
        {
            ReportId = entity.GithubContributionReportId,
            ProjectId = entity.ProjectId,
            PeriodStart = entity.PeriodStart,
            PeriodEnd = entity.PeriodEnd,
            TotalCommits = entity.TotalCommits,
            ContributorCount = entity.ContributorCount,
            ActiveContributorCount = entity.ActiveContributorCount,
            ExecutiveSummary = entity.ExecutiveSummary,
            ModelProvider = entity.ModelProvider,
            ModelName = entity.ModelName,
            CreatedAt = entity.CreatedAt
        };
    }

    private sealed class ContributorMetric
    {
        public string GithubUsername { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public int TotalCommits { get; set; }
        public int TotalAdditions { get; set; }
        public int TotalDeletions { get; set; }
    }

    private sealed class WeeklyMetric
    {
        public DateTime WeekStart { get; set; }
        public int Commits { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
    }
}