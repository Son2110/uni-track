using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// Controller for manually triggering GitHub data synchronization.
/// The sync process fetches contribution data from GitHub API and stores it locally
/// for improved query performance.
/// </summary>
[ApiController]
[Route("api/v1/github-sync")]
[Produces("application/json")]
[Authorize]
public class GithubSyncController : ControllerBase
{
    private readonly IGithubDataSyncService _githubDataSyncService;

    public GithubSyncController(IGithubDataSyncService githubDataSyncService)
    {
        _githubDataSyncService = githubDataSyncService;
    }

    /// <summary>
    /// Synchronize GitHub contribution data for all repositories
    /// </summary>
    /// <returns>Summary of the synchronization results</returns>
    /// <response code="200">Sync completed successfully</response>
    /// <response code="500">If an error occurred during sync</response>
    [HttpPost("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncAllRepositories()
    {
        var result = await _githubDataSyncService.SyncAllRepositoriesAsync();

        if (!result.Success)
            return StatusCode(StatusCodes.Status500InternalServerError, result);

        return Ok(result);
    }

    /// <summary>
    /// Synchronize GitHub contribution data for all repositories in a specific project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Summary of the synchronization results</returns>
    /// <response code="200">Sync completed successfully</response>
    /// <response code="404">If the project is not found or has no repositories</response>
    /// <response code="500">If an error occurred during sync</response>
    [HttpPost("projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncProjectRepositories(Guid projectId)
    {
        var result = await _githubDataSyncService.SyncProjectRepositoriesAsync(projectId);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Synchronize GitHub contribution data for a specific repository
    /// </summary>
    /// <param name="repoId">The unique identifier of the GitHub repository</param>
    /// <returns>Synchronization result for the repository</returns>
    /// <response code="200">Sync completed successfully</response>
    /// <response code="404">If the repository is not found</response>
    /// <response code="500">If an error occurred during sync</response>
    [HttpPost("repositories/{repoId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncRepository(Guid repoId)
    {
        var result = await _githubDataSyncService.SyncRepositoryAsync(repoId);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }
}
