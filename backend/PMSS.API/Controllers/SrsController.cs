using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for generating SRS documents from Jira data
/// </summary>
[ApiController]
[Produces("application/json")]
[Authorize]
public class SrsController : ControllerBase
{
    private readonly ISrsGenerationService _srsGenerationService;
    private readonly IAiSrsGenerationService _aiSrsGenerationService;
    private readonly IGithubContributionReportService _githubContributionReportService;

    public SrsController(
        ISrsGenerationService srsGenerationService,
        IAiSrsGenerationService aiSrsGenerationService,
        IGithubContributionReportService githubContributionReportService)
    {
        _srsGenerationService = srsGenerationService;
        _aiSrsGenerationService = aiSrsGenerationService;
        _githubContributionReportService = githubContributionReportService;
    }

    /// <summary>
    /// Generate an IEEE/ISO 29148 SRS document from Jira issues for a project (rule-based JSON)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Structured SRS document</returns>
    /// <response code="200">Returns the generated SRS document</response>
    /// <response code="404">If the project or Jira configuration is not found</response>
    /// <response code="502">If the Jira API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/srs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSrs(Guid projectId)
    {
        var result = await _srsGenerationService.GenerateSrsAsync(projectId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Generate an AI-powered SRS document as a .docx file from Jira issues for a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="usePaidModel">Set to true to use the paid OpenAI model (no token limit) for a more comprehensive SRS</param>
    /// <param name="modelOption">Optional model version for the AI generation</param>
    /// <returns>A downloadable .docx file containing the SRS document</returns>
    /// <response code="200">Returns the generated .docx file</response>
    /// <response code="404">If the project or Jira configuration is not found</response>
    /// <response code="502">If the Jira or AI API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/srs/docx")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSrsDocx(
        Guid projectId,
        [FromQuery] bool usePaidModel = false,
        [FromQuery] string? modelOption = null)
    {
        var result = await _aiSrsGenerationService.GenerateSrsDocxAsync(projectId, usePaidModel, modelOption);

        if (!result.Success)
            return NotFound(result);

        if (result.Data is not { Length: > 0 })
            return UnprocessableEntity(new { success = false, message = "The AI model returned empty content. Please try again." });

        return File(
            result.Data!,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"SRS_{projectId:N}.docx");
    }

    /// <summary>
    /// Generate an AI-powered SRS document as a Markdown file from Jira issues for a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="usePaidModel">Set to true to use the paid OpenAI model (no token limit) for a more comprehensive SRS</param>
    /// <param name="modelOption">Optional model version for the AI generation</param>
    /// <returns>A downloadable .md file containing the SRS document</returns>
    /// <response code="200">Returns the generated .md file</response>
    /// <response code="404">If the project or Jira configuration is not found</response>
    /// <response code="502">If the Jira or AI API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/srs/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSrsMarkdown(
        Guid projectId,
        [FromQuery] bool usePaidModel = false,
        [FromQuery] string? modelOption = null)
    {
        var result = await _aiSrsGenerationService.GenerateSrsMarkdownAsync(projectId, usePaidModel, modelOption);

        if (!result.Success)
            return NotFound(result);

        if (string.IsNullOrWhiteSpace(result.Data))
            return UnprocessableEntity(new { success = false, message = "The AI model returned empty content. Please try again." });

        var bytes = System.Text.Encoding.UTF8.GetBytes(result.Data!);
        return File(
            bytes,
            "text/markdown",
            $"SRS_{projectId:N}.md");
    }

    /// <summary>
    /// Generate an AI-powered GitHub project report as a Markdown file for a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="usePaidModel">Set to true to use the paid OpenAI model for a more comprehensive report</param>
    /// <param name="modelOption">Optional model version for the AI generation</param>
    /// <returns>A downloadable .md file containing the GitHub report</returns>
    /// <response code="200">Returns the generated .md file</response>
    /// <response code="404">If the project or GitHub repository configuration is not found</response>
    /// <response code="502">If the GitHub or AI API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/github-report/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateGithubReportMarkdown(
        Guid projectId,
        [FromQuery] bool usePaidModel = false,
        [FromQuery] string? modelOption = null,
        [FromQuery] int? recentWeeks = null,
        [FromQuery] bool includeMermaidDiagrams = false)
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        var result = await _githubContributionReportService.GenerateAndSaveAsync(
            projectId,
            userId,
            usePaidModel,
            modelOption,
            recentWeeks,
            includeMermaidDiagrams);

        if (!result.Success)
            return NotFound(result);

        if (result.Data == null || string.IsNullOrWhiteSpace(result.Data.MarkdownContent))
            return UnprocessableEntity(new { success = false, message = "The AI model returned empty content. Please try again." });

        var bytes = System.Text.Encoding.UTF8.GetBytes(result.Data.MarkdownContent);
        return File(
            bytes,
            "text/markdown",
            $"GitHub_Contribution_Report_{projectId:N}_{result.Data.ReportId:N}.md");
    }
}
