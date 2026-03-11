using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public SrsController(
        ISrsGenerationService srsGenerationService,
        IAiSrsGenerationService aiSrsGenerationService)
    {
        _srsGenerationService = srsGenerationService;
        _aiSrsGenerationService = aiSrsGenerationService;
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
    /// <returns>A downloadable .docx file containing the SRS document</returns>
    /// <response code="200">Returns the generated .docx file</response>
    /// <response code="404">If the project or Jira configuration is not found</response>
    /// <response code="502">If the Jira or AI API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/srs/docx")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSrsDocx(Guid projectId)
    {
        var result = await _aiSrsGenerationService.GenerateSrsDocxAsync(projectId);

        if (!result.Success)
            return NotFound(result);

        return File(
            result.Data!,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"SRS_{projectId:N}.docx");
    }

    /// <summary>
    /// Generate an AI-powered SRS document as a Markdown file from Jira issues for a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>A downloadable .md file containing the SRS document</returns>
    /// <response code="200">Returns the generated .md file</response>
    /// <response code="404">If the project or Jira configuration is not found</response>
    /// <response code="502">If the Jira or AI API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/srs/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSrsMarkdown(Guid projectId)
    {
        var result = await _aiSrsGenerationService.GenerateSrsMarkdownAsync(projectId);

        if (!result.Success)
            return NotFound(result);

        var bytes = System.Text.Encoding.UTF8.GetBytes(result.Data!);
        return File(
            bytes,
            "text/markdown",
            $"SRS_{projectId:N}.md");
    }
}
