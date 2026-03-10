using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Service that uses OpenAI to generate SRS documents from Jira issues
/// </summary>
public class SrsGeneratorService : ISrsGeneratorService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<SrsGeneratorService> _logger;
    private readonly string _modelId;

    public SrsGeneratorService(IConfiguration configuration, ILogger<SrsGeneratorService> logger)
    {
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured in appsettings.json");

        _modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";

        var endpoint = configuration["OpenAI:Endpoint"];

        if (!string.IsNullOrEmpty(endpoint))
        {
            // GitHub Models or Azure OpenAI compatible endpoint
            var options = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpoint) };
            var client = new OpenAI.OpenAIClient(new ApiKeyCredential(apiKey), options);
            _chatClient = client.GetChatClient(_modelId);
        }
        else
        {
            _chatClient = new ChatClient(_modelId, new ApiKeyCredential(apiKey));
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateSrsFromJiraAsync(string jiraIssuesJson, string projectName)
    {
        _logger.LogInformation("Generating SRS for project '{ProjectName}' using model '{ModelId}'", projectName, _modelId);

        var prompt = BuildSrsPrompt(jiraIssuesJson, projectName);

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert Software Requirements Analyst. Generate complete SRS documents in Markdown format following IEEE 830 standard."),
                new UserChatMessage(prompt)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            var srsContent = completion.Content[0].Text ?? string.Empty;

            // Clean up markdown code fences if AI wraps the output
            srsContent = CleanMarkdownOutput(srsContent);

            _logger.LogInformation("SRS generation completed for project '{ProjectName}'. Output length: {Length} chars", projectName, srsContent.Length);

            return srsContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SRS for project '{ProjectName}'", projectName);
            throw new InvalidOperationException($"AI generation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Builds the prompt for Gemini AI to generate an SRS from Jira issues
    /// </summary>
    private static string BuildSrsPrompt(string jiraIssuesJson, string projectName)
    {
        return $"""
            You are an expert Software Requirements Analyst. Your task is to analyze the Jira issues JSON data below and generate a complete Software Requirement Specification (SRS) document in Markdown format.

            ## Project Information
            - **Project Name:** {projectName}
            - **Date:** {DateTime.UtcNow:MMMM dd, yyyy}

            ## Instructions
            1. Analyze all Jira issues (summary, description, status) from the JSON data
            2. Group related issues into functional modules/features
            3. Generate a complete SRS document following the IEEE 830 standard template below
            4. For each Jira issue, create a corresponding Use Case with:
               - A clear use case name derived from the issue summary
               - Primary/Secondary actors inferred from the context
               - Main success scenario with detailed steps
               - Alternative scenarios and exceptions where applicable
            5. Infer non-functional requirements from the issues where possible
            6. Write in a professional, clear, and detailed manner
            7. Output ONLY the Markdown content — no code fences, no preamble

            ## SRS Template Structure

            # {projectName} — Software Requirement Specification

            ## 1. Introduction
            ### 1.1 Purpose
            ### 1.2 Definitions, Acronyms
            ### 1.3 References

            ## 2. Overall Description
            ### 2.1 Product Perspective
            ### 2.2 Business Process
            ### 2.3 User Classes

            ## 3. Functional Requirements
            ### 3.1 Use Case Diagram (describe textually)
            ### 3.2 Use Case Specifications (one per Jira issue or grouped logically)
            Each use case should include:
            - Use-case No., Name, Priority, Primary Actor, Secondary Actor
            - Description, Triggers, Preconditions, Post Conditions
            - Main Success Scenario (numbered steps)
            - Alternative Scenarios
            - Exceptions
            - Business Rules

            ## 4. Non-Functional Requirements
            ### 4.1 Usability
            ### 4.2 Reliability
            ### 4.3 Performance
            ### 4.4 Scalability

            ## 5. Supporting Information
            ### Appendix A — Business Rules Reference
            ### Appendix B — Integration Requirements

            ## Jira Issues JSON Data
            ```json
            {jiraIssuesJson}
            ```

            Generate the complete SRS document now in Markdown format.
            """;
    }

    /// <summary>
    /// Removes markdown code fences that AI might wrap the output in
    /// </summary>
    private static string CleanMarkdownOutput(string content)
    {
        content = content.Trim();

        // Remove leading ```markdown or ```
        if (content.StartsWith("```markdown", StringComparison.OrdinalIgnoreCase))
        {
            content = content["```markdown".Length..];
        }
        else if (content.StartsWith("```"))
        {
            content = content[3..];
        }

        // Remove trailing ```
        if (content.EndsWith("```"))
        {
            content = content[..^3];
        }

        return content.Trim();
    }
}
