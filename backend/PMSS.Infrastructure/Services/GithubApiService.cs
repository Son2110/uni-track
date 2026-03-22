using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

public class GithubApiService : IGithubApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string GitHubApiBaseUrl = "https://api.github.com";
    private const int MaxRetries = 5;
    private const int RetryDelayMs = 2000;
    private const int MaxPerPage = 100;

    public GithubApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GithubContributorStats?> GetRepositoryContributorStatsAsync(
        string owner,
        string repo,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/stats/contributors";

            var response = await GetWithRetryAsync(client, url);

            if (response == null || !response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content) || content == "[]")
                return new GithubContributorStats { Contributors = new List<GithubContributor>() };

            var contributors = JsonSerializer.Deserialize<List<GitHubContributorResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (contributors == null)
                return null;

            var stats = new GithubContributorStats
            {
                Contributors = contributors.Select(c => new GithubContributor
                {
                    Login = c.Author?.Login ?? string.Empty,
                    Email = c.Author?.Email,
                    TotalCommits = c.Total,
                    TotalAdditions = c.Weeks?.Sum(w => w.A) ?? 0,
                    TotalDeletions = c.Weeks?.Sum(w => w.D) ?? 0,
                    Weeks = c.Weeks?.Select(w => new GithubWeeklyStats
                    {
                        Timestamp = w.W,
                        Commits = w.C,
                        Additions = w.A,
                        Deletions = w.D
                    }).ToList() ?? new List<GithubWeeklyStats>()
                }).ToList()
            };

            return stats;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GithubWeeklyCommitActivity>?> GetRepositoryCommitActivityAsync(
        string owner,
        string repo,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/stats/commit_activity";

            var response = await GetWithRetryAsync(client, url);

            if (response == null || !response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content) || content == "[]")
                return new List<GithubWeeklyCommitActivity>();

            var activity = JsonSerializer.Deserialize<List<GitHubCommitActivityResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (activity == null)
                return null;

            return activity.Select(a => new GithubWeeklyCommitActivity
            {
                Timestamp = a.Week,
                Total = a.Total
            }).ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GithubCommitEntry>?> GetRepositoryCommitsAsync(
        string owner,
        string repo,
        int maxCount = 120,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var commits = await GetPagedResultsAsync<GitHubCommitListItem>(
                client,
                page => $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/commits?per_page={MaxPerPage}&page={page}",
                maxCount);

            if (commits == null)
                return null;

            return commits.Select(commit => new GithubCommitEntry
            {
                Sha = commit.Sha,
                AuthorLogin = commit.Author?.Login ?? commit.Commit?.Author?.Name ?? "unknown",
                Date = commit.Commit?.Author?.Date ?? DateTimeOffset.MinValue,
                Message = commit.Commit?.Message ?? string.Empty,
                Additions = 0,
                Deletions = 0
            }).ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GithubPullRequestEntry>?> GetRepositoryPullRequestsAsync(
        string owner,
        string repo,
        int maxCount = 80,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var pullRequests = await GetPagedResultsAsync<GitHubPullRequestResponse>(
                client,
                page => $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/pulls?state=all&sort=updated&direction=desc&per_page={MaxPerPage}&page={page}",
                maxCount);

            if (pullRequests == null)
                return null;

            return pullRequests.Select(pr => new GithubPullRequestEntry
            {
                Number = pr.Number,
                Title = pr.Title ?? string.Empty,
                AuthorLogin = pr.User?.Login ?? string.Empty,
                State = pr.State ?? string.Empty,
                IsMerged = pr.MergedAt.HasValue,
                CreatedAt = pr.CreatedAt,
                MergedAt = pr.MergedAt,
                ClosedAt = pr.ClosedAt,
                CommentCount = pr.Comments,
                ReviewCommentCount = pr.ReviewComments,
                ReviewCount = pr.Comments + pr.ReviewComments,
                ApprovedReviewCount = 0,
                ChangesRequestedReviewCount = 0
            }).ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GithubIssueEntry>?> GetRepositoryIssuesAsync(
        string owner,
        string repo,
        int maxCount = 100,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var allIssues = await GetPagedResultsAsync<GitHubIssueResponse>(
                client,
                page => $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/issues?state=all&sort=updated&direction=desc&per_page={MaxPerPage}&page={page}",
                maxCount);

            if (allIssues == null)
                return null;

            return allIssues
                .Where(i => i.PullRequest == null)
                .Take(maxCount)
                .Select(i => new GithubIssueEntry
                {
                    Number = i.Number,
                    Title = i.Title ?? string.Empty,
                    State = i.State ?? string.Empty,
                    AuthorLogin = i.User?.Login ?? string.Empty,
                    CreatedAt = i.CreatedAt,
                    ClosedAt = i.ClosedAt,
                    Assignees = i.Assignees?.Select(a => a.Login ?? string.Empty).Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? []
                })
                .ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GithubActivityEntry>?> GetRepositoryActivityLogsAsync(
        string owner,
        string repo,
        int maxCount = 100,
        string? accessToken = null)
    {
        try
        {
            var client = CreateHttpClient(accessToken);
            var events = await GetPagedResultsAsync<GitHubEventResponse>(
                client,
                page => $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/events?per_page={MaxPerPage}&page={page}",
                maxCount);

            if (events == null)
                return null;

            return events.Select(e => new GithubActivityEntry
            {
                EventType = e.Type ?? string.Empty,
                ActorLogin = e.Actor?.Login ?? string.Empty,
                Action = e.Payload?.Action,
                CreatedAt = e.CreatedAt
            }).ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<List<T>?> GetPagedResultsAsync<T>(HttpClient client, Func<int, string> urlFactory, int maxCount)
    {
        var results = new List<T>();
        var targetCount = Math.Max(1, maxCount);

        for (var page = 1; results.Count < targetCount; page++)
        {
            var response = await client.GetAsync(urlFactory(page));
            if (!response.IsSuccessStatusCode)
            {
                if (results.Count > 0)
                    break;
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var pageItems = JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (pageItems == null || pageItems.Count == 0)
                break;

            results.AddRange(pageItems.Take(targetCount - results.Count));

            if (pageItems.Count < MaxPerPage)
                break;
        }

        return results;
    }

    private async Task<GitHubCommitDetailsResponse?> GetCommitDetailsAsync(HttpClient client, string owner, string repo, string sha)
    {
        var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/commits/{sha}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GitHubCommitDetailsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task<List<GitHubPullRequestReviewResponse>> GetPullRequestReviewsAsync(HttpClient client, string owner, string repo, int pullNumber)
    {
        var allReviews = new List<GitHubPullRequestReviewResponse>();

        for (var page = 1; page <= 2; page++)
        {
            var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/pulls/{pullNumber}/reviews?per_page={MaxPerPage}&page={page}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                break;

            var content = await response.Content.ReadAsStringAsync();
            var pageReviews = JsonSerializer.Deserialize<List<GitHubPullRequestReviewResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (pageReviews == null || pageReviews.Count == 0)
                break;

            allReviews.AddRange(pageReviews);

            if (pageReviews.Count < MaxPerPage)
                break;
        }

        return allReviews;
    }

    private async Task<HttpResponseMessage?> GetWithRetryAsync(HttpClient client, string url)
    {
        HttpResponseMessage? response = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            response = await client.GetAsync(url);

            // GitHub returns 202 Accepted when stats are being computed
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                await Task.Delay(RetryDelayMs * (attempt + 1)); // Exponential backoff
                continue;
            }

            if (response.IsSuccessStatusCode)
                return response;

            // For other error codes, don't retry
            break;
        }

        return response;
    }

    private HttpClient CreateHttpClient(string? accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PMSS", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

        if (!string.IsNullOrEmpty(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client;
    }

    private class GitHubContributorResponse
    {
        [JsonPropertyName("author")]
        public GitHubAuthor? Author { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("weeks")]
        public List<GitHubWeekResponse>? Weeks { get; set; }
    }

    private class GitHubAuthor
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    private class GitHubWeekResponse
    {
        [JsonPropertyName("w")]
        public long W { get; set; }

        [JsonPropertyName("a")]
        public int A { get; set; }

        [JsonPropertyName("d")]
        public int D { get; set; }

        [JsonPropertyName("c")]
        public int C { get; set; }
    }

    private class GitHubCommitActivityResponse
    {
        [JsonPropertyName("week")]
        public long Week { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("days")]
        public List<int>? Days { get; set; }
    }

    private class GitHubCommitListItem
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public GitHubAuthor? Author { get; set; }

        [JsonPropertyName("commit")]
        public GitHubCommitInner? Commit { get; set; }
    }

    private class GitHubCommitInner
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("author")]
        public GitHubCommitAuthor? Author { get; set; }
    }

    private class GitHubCommitAuthor
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
    }

    private class GitHubCommitDetailsResponse
    {
        [JsonPropertyName("stats")]
        public GitHubCommitStats? Stats { get; set; }
    }

    private class GitHubCommitStats
    {
        [JsonPropertyName("additions")]
        public int Additions { get; set; }

        [JsonPropertyName("deletions")]
        public int Deletions { get; set; }
    }

    private class GitHubPullRequestResponse
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("user")]
        public GitHubAuthor? User { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonPropertyName("merged_at")]
        public DateTimeOffset? MergedAt { get; set; }

        [JsonPropertyName("comments")]
        public int Comments { get; set; }

        [JsonPropertyName("review_comments")]
        public int ReviewComments { get; set; }
    }

    private class GitHubPullRequestReviewResponse
    {
        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    private class GitHubIssueResponse
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("user")]
        public GitHubAuthor? User { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonPropertyName("assignees")]
        public List<GitHubAuthor>? Assignees { get; set; }

        [JsonPropertyName("pull_request")]
        public JsonElement? PullRequest { get; set; }
    }

    private class GitHubEventResponse
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("actor")]
        public GitHubAuthor? Actor { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("payload")]
        public GitHubEventPayload? Payload { get; set; }
    }

    private class GitHubEventPayload
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }
}
