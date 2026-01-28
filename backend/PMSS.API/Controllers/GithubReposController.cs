using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GithubReposController : ControllerBase
{
    private readonly IGithubRepoService _githubRepoService;

    public GithubReposController(IGithubRepoService githubRepoService)
    {
        _githubRepoService = githubRepoService;
    }

    /// <summary>
    /// Get all Github repositories with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GithubRepoFilterParams filterParams)
    {
        var result = await _githubRepoService.GetAllReposAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific Github repository by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _githubRepoService.GetRepoByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new Github repository (Any project member can create)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGithubRepoDto dto, [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _githubRepoService.CreateRepoAsync(dto, userId);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.GithubRepoId }, result);
    }

    /// <summary>
    /// Update an existing Github repository (Any project member can update)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGithubRepoDto dto, [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _githubRepoService.UpdateRepoAsync(id, dto, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a Github repository (Any project member can delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        var result = await _githubRepoService.DeleteRepoAsync(id, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all Github repositories for a specific course
    /// </summary>
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetByCourseId(Guid courseId)
    {
        var result = await _githubRepoService.GetReposByCourseIdAsync(courseId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all Github repositories for a specific user (projects they are part of)
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _githubRepoService.GetReposByUserIdAsync(userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Add a contributor to a Github repository
    /// Any project member can add any other project member, or users can add themselves if they are project members
    /// </summary>
    [HttpPost("{repoId}/contributors/{userId}")]
    public async Task<IActionResult> AddContributor(Guid repoId, Guid userId, [FromHeader(Name = "X-User-Id")] Guid addedByUserId)
    {
        var result = await _githubRepoService.AddContributorToRepoAsync(repoId, userId, addedByUserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Remove a contributor from a Github repository
    /// Any project member can remove any contributor, or users can remove themselves
    /// </summary>
    [HttpDelete("{repoId}/contributors/{userId}")]
    public async Task<IActionResult> RemoveContributor(Guid repoId, Guid userId, [FromHeader(Name = "X-User-Id")] Guid removedByUserId)
    {
        var result = await _githubRepoService.RemoveContributorFromRepoAsync(repoId, userId, removedByUserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Check if a user can manage a specific repository
    /// </summary>
    [HttpGet("{repoId}/can-manage")]
    public async Task<IActionResult> CanManageRepo(Guid repoId, [FromQuery] Guid userId)
    {
        var result = await _githubRepoService.CanUserManageRepoAsync(repoId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
