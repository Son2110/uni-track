namespace PMSS.Infrastructure.Configuration;

public class GitHubModelsSettings
{
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string GitHubToken { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o-mini";
}
