using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.GithubReport;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/v1/projects/{projectId:guid}/github-reports")]
[Produces("application/json")]
[Authorize]
public class GithubReportsController : ControllerBase
{
    private readonly IGithubContributionReportService _githubContributionReportService;

    public GithubReportsController(IGithubContributionReportService githubContributionReportService)
    {
        _githubContributionReportService = githubContributionReportService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateAndSave(
        Guid projectId,
        [FromBody] GenerateGithubContributionReportRequestDto? request = null)
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        var result = await _githubContributionReportService.GenerateAndSaveAsync(
            projectId,
            userId,
            request?.UsePaidModel ?? false,
            request?.ModelOption,
            request?.RecentWeeks,
            request?.IncludeMermaidDiagrams ?? false);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsByProject(Guid projectId, [FromQuery] int take = 20)
    {
        var normalizedTake = Math.Clamp(take, 1, 100);
        var result = await _githubContributionReportService.GetByProjectIdAsync(projectId, normalizedTake);
        return Ok(result);
    }

    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(Guid projectId)
    {
        var result = await _githubContributionReportService.GetLatestByProjectIdAsync(projectId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{reportId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid projectId, Guid reportId)
    {
        var result = await _githubContributionReportService.GetByIdAsync(reportId);
        if (!result.Success)
            return NotFound(result);

        if (result.Data?.ProjectId != projectId)
            return NotFound(new { success = false, message = "Report not found for this project" });

        return Ok(result);
    }

    [HttpGet("{reportId:guid}/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMarkdown(Guid projectId, Guid reportId)
    {
        var result = await _githubContributionReportService.GetByIdAsync(reportId);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        if (result.Data.ProjectId != projectId)
            return NotFound(new { success = false, message = "Report not found for this project" });

        var bytes = Encoding.UTF8.GetBytes(result.Data.MarkdownContent);
        return File(bytes, "text/markdown", $"GitHub_Contribution_Report_{projectId:N}_{reportId:N}.md");
    }

    [HttpGet("{reportId:guid}/mermaid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMermaid(Guid projectId, Guid reportId)
    {
        var result = await _githubContributionReportService.GetByIdAsync(reportId);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        if (result.Data.ProjectId != projectId)
            return NotFound(new { success = false, message = "Report not found for this project" });

        var mermaidBlocks = ExtractMermaidBlocks(result.Data.MarkdownContent);
        if (mermaidBlocks.Count == 0)
            return NotFound(new { success = false, message = "No Mermaid diagram found in this report" });

        var mermaidContent = string.Join($"{Environment.NewLine}{Environment.NewLine}", mermaidBlocks);
        var bytes = Encoding.UTF8.GetBytes(mermaidContent);
        return File(bytes, "text/plain", $"GitHub_Contribution_Report_{projectId:N}_{reportId:N}.mmd");
    }

    [HttpGet("{reportId:guid}/mermaid/blocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMermaidBlocks(Guid projectId, Guid reportId)
    {
        var result = await _githubContributionReportService.GetByIdAsync(reportId);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        if (result.Data.ProjectId != projectId)
            return NotFound(new { success = false, message = "Report not found for this project" });

        var mermaidBlocks = ExtractMermaidBlocks(result.Data.MarkdownContent);
        if (mermaidBlocks.Count == 0)
            return NotFound(new { success = false, message = "No Mermaid diagram found in this report" });

        return Ok(new
        {
            success = true,
            message = "Mermaid blocks extracted successfully",
            data = new
            {
                reportId,
                projectId,
                count = mermaidBlocks.Count,
                blocks = mermaidBlocks
            }
        });
    }

    private static List<string> ExtractMermaidBlocks(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return [];

        var matches = Regex.Matches(markdown, @"```mermaid\s*(?<content>[\s\S]*?)```", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
            return [];

        var sections = new List<string>();
        foreach (Match match in matches)
        {
            var block = match.Groups["content"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(block))
                sections.Add(block);
        }

        return sections;
    }
}