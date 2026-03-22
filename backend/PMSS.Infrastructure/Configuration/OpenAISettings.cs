namespace PMSS.Infrastructure.Configuration;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o";
    public string Endpoint { get; set; } = "https://api.openai.com/v1";
    public int NetworkTimeoutInSeconds { get; set; } = 600;
    public int MaxOutputTokens { get; set; } = 32768;

    // Paid model options
    public string FastModelName { get; set; } = "gpt-4.1-mini";
    public string BalancedModelName { get; set; } = "gpt-4o";
    public string QualityModelName { get; set; } = "gpt-5.1";

    // Task defaults when caller does not specify modelOption
    public string DefaultSrsModelOption { get; set; } = "quality";
    public string DefaultGithubReportModelOption { get; set; } = "balanced";
}
