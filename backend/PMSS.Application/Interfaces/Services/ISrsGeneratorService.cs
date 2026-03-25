namespace PMSS.Application.Interfaces.Services;

public interface ISrsGeneratorService
{
    Task<string> GenerateSrsFromJiraAsync(string jiraIssuesJson, string projectName);
    Task<string> SaveSrsToFileAsync(string srsContent, Guid projectId, string projectName);
    string? GetSrsFilePath(string fileName);
    string[] GetSrsFilesByProject(Guid projectId);
}
