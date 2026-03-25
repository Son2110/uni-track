using PMSS.Application.DTOs.GithubRepo;

namespace PMSS.Application.Interfaces.Services;

public interface IGithubApiService
{
    Task<GithubContributorStats?> GetRepositoryContributorStatsAsync(string owner, string repo, string? accessToken = null);
    Task<List<GithubWeeklyCommitActivity>?> GetRepositoryCommitActivityAsync(string owner, string repo, string? accessToken = null);
    Task<List<GithubCommitEntry>?> GetRepositoryCommitsAsync(string owner, string repo, int maxCount = 120, string? accessToken = null);
    Task<List<GithubPullRequestEntry>?> GetRepositoryPullRequestsAsync(string owner, string repo, int maxCount = 80, string? accessToken = null);
    Task<List<GithubIssueEntry>?> GetRepositoryIssuesAsync(string owner, string repo, int maxCount = 100, string? accessToken = null);
    Task<List<GithubActivityEntry>?> GetRepositoryActivityLogsAsync(string owner, string repo, int maxCount = 100, string? accessToken = null);
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

public class GithubCommitEntry
{
    public string Sha { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Additions { get; set; }
    public int Deletions { get; set; }
}

public class GithubPullRequestEntry
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public bool IsMerged { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? MergedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public int CommentCount { get; set; }
    public int ReviewCommentCount { get; set; }
    public int ReviewCount { get; set; }
    public int ApprovedReviewCount { get; set; }
    public int ChangesRequestedReviewCount { get; set; }
}

public class GithubIssueEntry
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public List<string> Assignees { get; set; } = new();
}

public class GithubActivityEntry
{
    public string EventType { get; set; } = string.Empty;
    public string ActorLogin { get; set; } = string.Empty;
    public string? Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
