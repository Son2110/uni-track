using System.Net.Http.Headers;
using System.Text;
using System.Web;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;

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

        if (string.IsNullOrWhiteSpace(jiraConfig.JiraUrl))
            throw new InvalidOperationException("Jira URL is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.Email))
            throw new InvalidOperationException("Jira Email is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.ApiToken))
            throw new InvalidOperationException("Jira API Token is not configured");

        if (string.IsNullOrWhiteSpace(jiraConfig.ProjectKey))
            throw new InvalidOperationException("Jira Project Key is not configured");

        var client = _httpClientFactory.CreateClient();

        // Set up Basic Authentication using shared credentials from JiraConfig
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraConfig.Email}:{jiraConfig.ApiToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Build the Jira search endpoint URL
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
