using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing GitHub repository resources.
/// Supports both standalone access and nested access under projects.
/// </summary>
[ApiController]
[Produces("application/json")]
[Authorize]
public class GithubReposController : ControllerBase
{
    private readonly IGithubRepoService _githubRepoService;

    public GithubReposController(IGithubRepoService githubRepoService)
    {
        _githubRepoService = githubRepoService;
    }

    /// <summary>
    /// Retrieve all GitHub repositories with optional filtering.
    /// Supports filtering by courseId and userId via query parameters.
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering (courseId, userId) and pagination</param>
    /// <returns>A collection of GitHub repositories</returns>
    /// <response code="200">Returns the list of repositories</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet("api/v1/github-repos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] GithubRepoFilterParams filterParams)
    {
        var result = await _githubRepoService.GetAllReposAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific GitHub repository by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the repository</param>
    /// <returns>The requested repository resource</returns>
    /// <response code="200">Returns the repository</response>
    /// <response code="404">If the repository is not found</response>
    [HttpGet("api/v1/github-repos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _githubRepoService.GetRepoByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new GitHub repository resource
    /// </summary>
    /// <param name="dto">The repository creation data</param>
    /// <returns>The newly created repository</returns>
    /// <response code="201">Returns the newly created repository</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the user is not authorized to create a repository</response>
    [HttpPost("api/v1/github-repos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateGithubRepoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _githubRepoService.CreateRepoAsync(dto, userId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.GithubRepoId }, result);
    }

    /// <summary>
    /// Replace an existing GitHub repository resource
    /// </summary>
    /// <param name="id">The unique identifier of the repository to update</param>
    /// <param name="dto">The complete repository data for replacement</param>
    /// <returns>The updated repository resource</returns>
    /// <response code="200">Returns the updated repository</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the user is not authorized to update</response>
    /// <response code="404">If the repository is not found</response>
    [HttpPut("api/v1/github-repos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGithubRepoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _githubRepoService.UpdateRepoAsync(id, dto, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a GitHub repository resource
    /// </summary>
    /// <param name="id">The unique identifier of the repository to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Repository deleted successfully</response>
    /// <response code="403">If the user is not authorized to delete</response>
    /// <response code="404">If the repository is not found</response>
    [HttpDelete("api/v1/github-repos/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _githubRepoService.DeleteRepoAsync(id, userId);

        if (!result.Success)
            return NotFound(result);

        return NoContent();
    }

    /// <summary>
    /// Retrieve all GitHub repositories for a specific course (nested resource)
    /// </summary>
    /// <param name="courseId">The unique identifier of the course</param>
    /// <returns>Collection of repositories for the course</returns>
    /// <response code="200">Returns the repositories</response>
    /// <response code="404">If the course is not found</response>
    [HttpGet("api/v1/courses/{courseId:guid}/github-repos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCourseId(Guid courseId)
    {
        var result = await _githubRepoService.GetReposByCourseIdAsync(courseId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve all GitHub repositories for a specific user (nested resource)
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Collection of repositories for the user</returns>
    /// <response code="200">Returns the repositories</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("api/v1/users/{userId:guid}/github-repos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _githubRepoService.GetReposByUserIdAsync(userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Add a contributor to a GitHub repository (create contributor sub-resource)
    /// </summary>
    /// <param name="repoId">The unique identifier of the repository</param>
    /// <param name="userId">The unique identifier of the user to add as contributor</param>
    /// <returns>Success status</returns>
    /// <response code="201">Contributor added successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="403">If the user is not authorized</response>
    /// <response code="409">If the user is already a contributor</response>
    [HttpPost("api/v1/github-repos/{repoId:guid}/contributors/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddContributor(Guid repoId, Guid userId)
    {
        var addedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _githubRepoService.AddContributorToRepoAsync(repoId, userId, addedByUserId);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Remove a contributor from a GitHub repository (delete contributor sub-resource)
    /// </summary>
    /// <param name="repoId">The unique identifier of the repository</param>
    /// <param name="userId">The unique identifier of the user to remove</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Contributor removed successfully</response>
    /// <response code="403">If the user is not authorized</response>
    /// <response code="404">If the contributor is not found</response>
    [HttpDelete("api/v1/github-repos/{repoId:guid}/contributors/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveContributor(Guid repoId, Guid userId)
    {
        var removedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _githubRepoService.RemoveContributorFromRepoAsync(repoId, userId, removedByUserId);

        if (!result.Success)
            return NotFound(result);

        return NoContent();
    }

    /// <summary>
    /// Check if a user can manage a specific repository
    /// </summary>
    /// <param name="repoId">The unique identifier of the repository</param>
    /// <param name="userId">The unique identifier of the user to check</param>
    /// <returns>Boolean indicating management permission</returns>
    /// <response code="200">Returns the permission status</response>
    /// <response code="404">If the repository is not found</response>
    [HttpGet("api/v1/github-repos/{repoId:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CanManageRepo(Guid repoId, [FromQuery] Guid userId)
    {
        var result = await _githubRepoService.CanUserManageRepoAsync(repoId, userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
