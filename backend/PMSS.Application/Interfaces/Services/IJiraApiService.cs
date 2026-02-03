namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service interface for interacting with Jira API
/// </summary>
public interface IJiraApiService
{
    /// <summary>
    /// Fetches raw Jira issues as JSON string for a given project
    /// </summary>
    /// <param name="projectId">The PMSS project ID linked to a Jira configuration</param>
    /// <param name="userEmail">Email of authenticated user (from JWT/DB) for Jira authentication</param>
    /// <returns>Raw JSON string containing Jira issues</returns>
    Task<string> FetchRawJiraIssuesAsync(Guid projectId, string userEmail);
}
