using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Service for interacting with Jira REST API (v3)
/// Uses the new /rest/api/3/search/jql endpoint
/// </summary>
public class JiraApiService : IJiraApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJiraConfigRepository _jiraConfigRepository;

    public JiraApiService(
        IHttpClientFactory httpClientFactory,
        IJiraConfigRepository jiraConfigRepository)
    {
        _httpClientFactory = httpClientFactory;
        _jiraConfigRepository = jiraConfigRepository;
    }

    /// <inheritdoc />
    public async Task<string> FetchRawJiraIssuesAsync(Guid projectId)
    {
        var jiraConfig = await _jiraConfigRepository.GetActiveConfigByProjectIdAsync(projectId);

        if (jiraConfig == null)
        {
            throw new InvalidOperationException($"No active Jira configuration found for project ID: {projectId}");
        }

        if (string.IsNullOrWhiteSpace(jiraConfig.JiraUrl) ||
            string.IsNullOrWhiteSpace(jiraConfig.Email) ||
            string.IsNullOrWhiteSpace(jiraConfig.ApiToken) ||
            string.IsNullOrWhiteSpace(jiraConfig.ProjectKey))
        {
            throw new InvalidOperationException("Jira configuration is incomplete. Please ensure JiraUrl, Email, ApiToken, and ProjectKey are configured.");
        }

        var client = _httpClientFactory.CreateClient();

        // Set up Basic Authentication (Email:ApiToken encoded in Base64)
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraConfig.Email}:{jiraConfig.ApiToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Build the new Jira search endpoint URL (migrated from /rest/api/3/search to /rest/api/3/search/jql)
        // Reference: https://developer.atlassian.com/changelog/#CHANGE-2046
        var jql = $"project = {jiraConfig.ProjectKey} ORDER BY created DESC";
        var fields = "summary,description,status";
        
        var searchUrl = $"{jiraConfig.JiraUrl.TrimEnd('/')}/rest/api/3/search/jql" +
                        $"?jql={HttpUtility.UrlEncode(jql)}" +
                        $"&fields={fields}";

        var response = await client.GetAsync(searchUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Jira API request failed with status {response.StatusCode}: {errorContent}");
        }

        var rawJsonResponse = await response.Content.ReadAsStringAsync();
        return rawJsonResponse;
    }
}
