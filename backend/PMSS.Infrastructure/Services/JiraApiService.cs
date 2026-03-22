using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Infrastructure.Configuration;
using PMSS.Infrastructure.Utilities;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Service for interacting with Jira REST API (v3)
/// Uses the /rest/api/3/search/jql endpoint
/// Email and API Token are from JiraConfig (shared credentials)
/// </summary>
public class JiraApiService : IJiraApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJiraConfigRepository _jiraConfigRepository;
    private readonly string _encryptionKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JiraApiService(
        IHttpClientFactory httpClientFactory,
        IJiraConfigRepository jiraConfigRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _httpClientFactory = httpClientFactory;
        _jiraConfigRepository = jiraConfigRepository;
        _encryptionKey = jwtSettings.Value.SecretKey;
    }

    /// <inheritdoc />
    public async Task<string> FetchRawJiraIssuesAsync(Guid projectId)
    {
        var jiraConfig = await _jiraConfigRepository.GetActiveConfigByProjectIdAsync(projectId);

        if (jiraConfig == null)
        {
            throw new InvalidOperationException($"No active Jira configuration found for project ID: {projectId}");
        }

        if (string.IsNullOrWhiteSpace(jiraConfig.JiraUrl))
            throw new InvalidOperationException("Jira URL is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.Email))
            throw new InvalidOperationException("Jira Email is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.ApiToken))
            throw new InvalidOperationException("Jira API Token is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.ProjectKey))
            throw new InvalidOperationException("Jira Project Key is not configured");

        var client = _httpClientFactory.CreateClient();

        // Decrypt the API token before use
        var decryptedToken = AesEncryptionHelper.Decrypt(jiraConfig.ApiToken, _encryptionKey);

        // Set up Basic Authentication (Email:ApiToken encoded in Base64)
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraConfig.Email}:{decryptedToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Build the Jira search endpoint URL
        var jql = $"project = {jiraConfig.ProjectKey} ORDER BY created DESC";
        var fields = "summary,description,status,issuetype,priority,labels,components,assignee,created,updated,issuelinks,fixVersions,parent,comment,resolution,subtasks,environment";

        var baseUrl = $"{jiraConfig.JiraUrl.TrimEnd('/')}/rest/api/3/search/jql";
        var allIssues = new List<JsonElement>();
        int startAt = 0;
        int total;

        // Paginate through all Jira issues to ensure nothing is missed
        do
        {
            var searchUrl = $"{baseUrl}" +
                            $"?jql={HttpUtility.UrlEncode(jql)}" +
                            $"&fields={fields}" +
                            $"&startAt={startAt}" +
                            $"&maxResults=100";

            var response = await client.GetAsync(searchUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Jira API request failed with status {response.StatusCode}: {errorContent}");
            }

            var rawPage = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(rawPage);
            var root = doc.RootElement;

            total = root.TryGetProperty("total", out var totalEl) ? totalEl.GetInt32() : 0;

            if (root.TryGetProperty("issues", out var issuesArray) && issuesArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var issue in issuesArray.EnumerateArray())
                    allIssues.Add(issue.Clone());
            }

            startAt += 100;
        } while (startAt < total);

        // Return a combined JSON with all issues
        return JsonSerializer.Serialize(new { issues = allIssues, total = allIssues.Count }, JsonOptions);
    }
}
