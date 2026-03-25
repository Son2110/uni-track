namespace PMSS.Domain.Entities;

public class GithubContributionReport
{
    public Guid GithubContributionReportId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? GeneratedByUserId { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
    public int ContributorCount { get; set; }
    public int ActiveContributorCount { get; set; }

    public string ModelProvider { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string InsightsJson { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual User? GeneratedByUser { get; set; }
}