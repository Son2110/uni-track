namespace PMSS.Application.DTOs.GithubReport;

public class GenerateGithubContributionReportRequestDto
{
    public bool UsePaidModel { get; set; }
    public string? ModelOption { get; set; }
    public int? RecentWeeks { get; set; }
    public bool IncludeMermaidDiagrams { get; set; }
}

public class GithubContributionReportSummaryDto
{
    public Guid ReportId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalCommits { get; set; }
    public int ContributorCount { get; set; }
    public int ActiveContributorCount { get; set; }
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string ModelProvider { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GithubContributionReportDto : GithubContributionReportSummaryDto
{
    public string InsightsJson { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;
}