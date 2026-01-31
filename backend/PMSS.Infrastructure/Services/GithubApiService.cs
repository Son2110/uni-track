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
}
