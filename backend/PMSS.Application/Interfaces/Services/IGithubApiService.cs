using PMSS.Application.DTOs.GithubRepo;

namespace PMSS.Application.Interfaces.Services;

public interface IGithubApiService
{
    Task<GithubContributorStats?> GetRepositoryContributorStatsAsync(string owner, string repo, string? accessToken = null);
    Task<List<GithubWeeklyCommitActivity>?> GetRepositoryCommitActivityAsync(string owner, string repo, string? accessToken = null);
}

public class GithubContributorStats
{
    public List<GithubContributor> Contributors { get; set; } = new();
}

public class GithubContributor
{
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
    public List<GithubWeeklyStats> Weeks { get; set; } = new();
}

public class GithubWeeklyStats
{
    public long Timestamp { get; set; }
    public int Commits { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
}

public class GithubWeeklyCommitActivity
{
    public long Timestamp { get; set; }
    public int Total { get; set; }
}
