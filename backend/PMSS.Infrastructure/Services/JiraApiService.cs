using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Infrastructure.Configuration;
using PMSS.Infrastructure.Utilities;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Service for interacting with Jira REST API (v3)
/// Uses the new /rest/api/3/search/jql endpoint
/// </summary>
public class JiraApiService : IJiraApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJiraConfigRepository _jiraConfigRepository;
    private readonly string _encryptionKey;

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

        if (string.IsNullOrWhiteSpace(jiraConfig.JiraUrl) ||
            string.IsNullOrWhiteSpace(jiraConfig.Email) ||
            string.IsNullOrWhiteSpace(jiraConfig.ApiToken) ||
            string.IsNullOrWhiteSpace(jiraConfig.ProjectKey))
        {
            throw new InvalidOperationException("Jira configuration is incomplete. Please ensure JiraUrl, Email, ApiToken, and ProjectKey are configured.");
        }

        var client = _httpClientFactory.CreateClient();

        // Decrypt the API token before use
        var decryptedToken = AesEncryptionHelper.Decrypt(jiraConfig.ApiToken, _encryptionKey);

        // Set up Basic Authentication (Email:ApiToken encoded in Base64)
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraConfig.Email}:{decryptedToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Build the new Jira search endpoint URL (migrated from /rest/api/3/search to /rest/api/3/search/jql)
        // Reference: https://developer.atlassian.com/changelog/#CHANGE-2046
        var jql = $"project = {jiraConfig.ProjectKey} ORDER BY created DESC";
        var fields = "summary,description,status,issuetype,priority,labels,components,assignee,created,updated,issuelinks,fixVersions,parent,comment";

        var searchUrl = $"{jiraConfig.JiraUrl.TrimEnd('/')}/rest/api/3/search/jql" +
                        $"?jql={HttpUtility.UrlEncode(jql)}" +
                        $"&fields={fields}" +
                        $"&maxResults=100";

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
