namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service interface for interacting with Jira API
/// </summary>
public interface IJiraApiService
{
    /// <summary>
    /// Fetches raw Jira issues as JSON string for a given project
    /// Uses email and API token from JiraConfig (shared credentials)
    /// </summary>
    /// <param name="projectId">The PMSS project ID linked to a Jira configuration</param>
    /// <returns>Raw JSON string containing Jira issues</returns>
    Task<string> FetchRawJiraIssuesAsync(Guid projectId);
}
