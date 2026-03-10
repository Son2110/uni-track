namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service interface for generating SRS documents from Jira issues using AI
/// </summary>
public interface ISrsGeneratorService
{
    /// <summary>
    /// Generates an SRS (Software Requirement Specification) document
    /// from raw Jira issues JSON using Google Gemini AI
    /// </summary>
    /// <param name="jiraIssuesJson">Raw JSON string of Jira issues</param>
    /// <param name="projectName">The project name for the SRS header</param>
    /// <returns>SRS document in Markdown format</returns>
    Task<string> GenerateSrsFromJiraAsync(string jiraIssuesJson, string projectName);
}
