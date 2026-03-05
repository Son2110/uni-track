using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.JiraConfig;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing Jira integration resources.
/// Jira configuration and issues are nested under projects following RESTful resource hierarchy.
/// </summary>
[ApiController]
[Produces("application/json")]
[Authorize]
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
    /// Create Jira configuration for a project (create sub-resource)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="dto">Jira configuration details</param>
    /// <returns>Created Jira configuration</returns>
    /// <response code="201">Returns the created configuration</response>
    /// <response code="400">If validation fails</response>
    /// <response code="404">If the project is not found</response>
    /// <response code="409">If configuration already exists</response>
    [HttpPost("api/v1/projects/{projectId:guid}/jira-config")]
    [ProducesResponseType(typeof(JiraConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateConfig(Guid projectId, [FromBody] CreateJiraConfigDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verify project exists
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
        {
            return NotFound(new { error = $"Project with ID {projectId} not found." });
        }

        // Check if config already exists for this project
        var existing = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);
        if (existing != null)
        {
            return Conflict(new { error = "Jira configuration already exists for this project. Use PUT to update." });
        }

        var config = new JiraConfig
        {
            JiraConfigId = Guid.NewGuid(),
            ProjectId = projectId,
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
    /// Retrieve Jira configuration for a project (get sub-resource)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Jira configuration (API token masked)</returns>
    /// <response code="200">Returns the configuration</response>
    /// <response code="404">If no configuration found</response>
    [HttpGet("api/v1/projects/{projectId:guid}/jira-config")]
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
            UpdatedAt = config.UpdatedAt
        });
    }

    /// <summary>
    /// Update Jira configuration for a project (partial update using PATCH)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="dto">Fields to update (only provided fields will be updated)</param>
    /// <returns>The updated configuration</returns>
    /// <response code="200">Configuration updated successfully</response>
    /// <response code="400">If validation fails</response>
    /// <response code="404">If no configuration found</response>
    [HttpPatch("api/v1/projects/{projectId:guid}/jira-config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConfig(Guid projectId, [FromBody] UpdateJiraConfigDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var config = await _unitOfWork.JiraConfigs.GetByProjectIdAsync(projectId);

        if (config == null)
            return NotFound(new { error = $"No Jira configuration found for project ID: {projectId}" });

        // Update only provided fields (PATCH semantics)
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
    /// Delete Jira configuration for a project (delete sub-resource)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Configuration deleted successfully</response>
    /// <response code="404">If no configuration found</response>
    [HttpDelete("api/v1/projects/{projectId:guid}/jira-config")]
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
    /// Test Jira connection for a project (action on sub-resource)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Connection test result</returns>
    /// <response code="200">Connection successful</response>
    /// <response code="400">Connection failed</response>
    /// <response code="404">If no configuration found</response>
    [HttpPost("api/v1/projects/{projectId:guid}/jira-config/test-connection")]
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
    /// Retrieve Jira issues for a project (get nested resource)
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Raw JSON response from Jira containing issues</returns>
    /// <response code="200">Returns the Jira issues</response>
    /// <response code="400">If the Jira configuration is invalid</response>
    /// <response code="404">If no Jira configuration is found</response>
    /// <response code="502">If the Jira API request fails</response>
    [HttpGet("api/v1/projects/{projectId:guid}/jira-issues")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetJiraIssues(Guid projectId)
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
}
