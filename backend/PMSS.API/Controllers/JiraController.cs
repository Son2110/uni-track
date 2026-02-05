using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.JiraConfig;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.API.Controllers;

/// <summary>
/// Controller for Jira integration endpoints
/// Admin creates JiraConfig with shared email and API token
/// Anyone can fetch Jira issues using the shared credentials
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JiraController : ControllerBase
{
    private readonly IJiraApiService _jiraApiService;
    private readonly IUnitOfWork _unitOfWork;

    public JiraController(IJiraApiService jiraApiService, IUnitOfWork unitOfWork)
    {
        _jiraApiService = jiraApiService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create Jira configuration for a project
    /// Admin creates this once with shared email and API token
    /// </summary>
    /// <param name="dto">Jira configuration details</param>
    /// <returns>Created Jira configuration</returns>
    /// <response code="201">Returns the created configuration</response>
    /// <response code="400">If configuration already exists or validation fails</response>
    [HttpPost("config")]
    [ProducesResponseType(typeof(JiraConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConfig([FromBody] CreateJiraConfigDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if config already exists for this project
        var existing = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(dto.ProjectId);
        if (existing != null)
        {
            return BadRequest(new { error = "Jira configuration already exists for this project. Use PUT to update." });
        }

        // Verify project exists
        var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
        if (project == null)
        {
            return BadRequest(new { error = $"Project with ID {dto.ProjectId} not found." });
        }

        var config = new JiraConfig
        {
            JiraConfigId = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            JiraUrl = dto.JiraUrl.TrimEnd('/'),
            Email = dto.Email,
            ApiToken = dto.ApiToken,
            ProjectKey = dto.ProjectKey.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.JiraConfigs.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();

        var response = new JiraConfigDto
        {
            JiraConfigId = config.JiraConfigId,
            ProjectId = config.ProjectId,
            ProjectName = project.Name,
            JiraUrl = config.JiraUrl,
            Email = config.Email,
            ApiTokenMasked = MaskToken(config.ApiToken),
            ProjectKey = config.ProjectKey,
            IsActive = config.IsActive,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };

        return CreatedAtAction(nameof(GetConfig), new { projectId = config.ProjectId }, response);
    }

    /// <summary>
    /// Get Jira configuration for a project
    /// </summary>
    /// <param name="projectId">The PMSS project ID</param>
    /// <returns>Jira configuration (API token masked)</returns>
    /// <response code="200">Returns the configuration</response>
    /// <response code="404">If no configuration found</response>
    [HttpGet("config/{projectId:guid}")]
    [ProducesResponseType(typeof(JiraConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfig(Guid projectId)
    {
        var config = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);

        if (config == null)
            return NotFound(new { error = $"No Jira configuration found for project ID: {projectId}" });

        return Ok(new JiraConfigDto
        {
            JiraConfigId = config.JiraConfigId,
            ProjectId = config.ProjectId,
            ProjectName = config.Project?.Name ?? string.Empty,
            JiraUrl = config.JiraUrl,
            Email = config.Email,
            ApiTokenMasked = MaskToken(config.ApiToken),
            ProjectKey = config.ProjectKey,
            IsActive = config.IsActive,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt,
            CreatedByUserId = config.CreatedByUserId
        });
    }

    /// <summary>
    /// Update Jira configuration for a project
    /// </summary>
    /// <param name="projectId">The PMSS project ID</param>
    /// <param name="dto">Fields to update (only provided fields will be updated)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Configuration updated successfully</response>
    /// <response code="404">If no configuration found</response>
    [HttpPut("config/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConfig(Guid projectId, [FromBody] UpdateJiraConfigDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var config = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);

        if (config == null)
            return NotFound(new { error = $"No Jira configuration found for project ID: {projectId}" });

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(dto.JiraUrl))
            config.JiraUrl = dto.JiraUrl.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(dto.Email))
            config.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.ApiToken))
            config.ApiToken = dto.ApiToken;

        if (!string.IsNullOrWhiteSpace(dto.ProjectKey))
            config.ProjectKey = dto.ProjectKey.ToUpperInvariant();

        if (dto.IsActive.HasValue)
            config.IsActive = dto.IsActive.Value;

        config.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.JiraConfigs.Update(config);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Configuration updated successfully" });
    }

    /// <summary>
    /// Delete Jira configuration for a project
    /// </summary>
    /// <param name="projectId">The PMSS project ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Configuration deleted successfully</response>
    /// <response code="404">If no configuration found</response>
    [HttpDelete("config/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfig(Guid projectId)
    {
        var config = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);

        if (config == null)
            return NotFound(new { error = $"No Jira configuration found for project ID: {projectId}" });

        _unitOfWork.JiraConfigs.Remove(config);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Test Jira connection for a project
    /// Uses shared email and API token from JiraConfig
    /// </summary>
    /// <param name="projectId">The PMSS project ID</param>
    /// <returns>Connection test result</returns>
    /// <response code="200">Connection successful</response>
    /// <response code="400">Connection failed</response>
    /// <response code="404">If no configuration found</response>
    [HttpPost("config/{projectId:guid}/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestConnection(Guid projectId)
    {
        try
        {
            var result = await _jiraApiService.FetchRawJiraIssuesAsync(projectId);
            return Ok(new { message = "Connection successful", connected = true });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No active Jira configuration found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message, connected = false });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { error = "Connection failed", details = ex.Message, connected = false });
        }
    }

    /// <summary>
    /// Fetches raw Jira issues for a specific project
    /// Uses shared email and API token from JiraConfig for authentication
    /// </summary>
    /// <param name="projectId">The PMSS project ID linked to a Jira configuration</param>
    /// <returns>Raw JSON response from Jira containing issues</returns>
    /// <response code="200">Returns the raw Jira issues JSON</response>
    /// <response code="400">If the Jira configuration is invalid or incomplete</response>
    /// <response code="404">If no Jira configuration is found for the project</response>
    /// <response code="502">If the Jira API request fails</response>
    [HttpGet("fetch/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> FetchJiraIssues(Guid projectId)
    {
        try
        {
            var rawJson = await _jiraApiService.FetchRawJiraIssuesAsync(projectId);
            return Content(rawJson, "application/json");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No active Jira configuration found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to communicate with Jira API", details = ex.Message });
        }
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 8)
            return "********";

        return token[..4] + "****" + token[^4..];
    }

    /// <summary>
    /// Get setup guide for Jira configuration
    /// </summary>
    [HttpGet("setup-guide")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSetupGuide()
    {
        return Ok(new
        {
            title = "Jira Integration Setup Guide",
            description = "One person creates the Jira config with shared credentials. Anyone can then use the fetch feature.",
            steps = new[]
            {
                new { step = 1, title = "Get your Jira URL", description = "Your Jira URL looks like: https://yourteam.atlassian.net" },
                new { step = 2, title = "Get API Token", description = "Go to https://id.atlassian.com/manage-profile/security/api-tokens and create a new token" },
                new { step = 3, title = "Find Project Key", description = "Your project key is the prefix in issue IDs (e.g., 'SP' from 'SP-123')" },
                new { step = 4, title = "Invite team members", description = "Add team members to your Jira workspace using their email addresses" }
            },
            links = new
            {
                createToken = "https://id.atlassian.com/manage-profile/security/api-tokens",
                jiraHelp = "https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/"
            },
            note = "Only ONE person needs to create the API token and config. All team members can use the fetch feature."
        });
    }
}
