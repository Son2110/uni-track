using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Project;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing project resources
/// </summary>
[ApiController]
[Route("api/v1/projects")]
[Produces("application/json")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Retrieve a collection of projects with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering and pagination</param>
    /// <returns>A paginated collection of projects</returns>
    /// <response code="200">Returns the list of projects</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] ProjectFilterParams filterParams)
    {
        var result = await _projectService.GetAllProjectsAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific project by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the project</param>
    /// <returns>The requested project resource</returns>
    /// <response code="200">Returns the project</response>
    /// <response code="404">If the project is not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projectService.GetProjectByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new project resource
    /// </summary>
    /// <param name="dto">The project creation data</param>
    /// <returns>The newly created project</returns>
    /// <response code="201">Returns the newly created project</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _projectService.CreateProjectAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ProjectId }, result);
    }

    /// <summary>
    /// Replace an existing project resource
    /// </summary>
    /// <param name="id">The unique identifier of the project to update</param>
    /// <param name="dto">The complete project data for replacement</param>
    /// <returns>The updated project resource</returns>
    /// <response code="200">Returns the updated project</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the project is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _projectService.UpdateProjectAsync(id, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a project resource
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="404">If the project is not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _projectService.DeleteProjectAsync(id);

        if (!result.Success)
            return NotFound(result);

        return NoContent();
    }

    /// <summary>
    /// Retrieve GitHub contributions for a specific project (nested resource)
    /// </summary>
    /// <param name="id">The unique identifier of the project</param>
    /// <returns>GitHub contribution data for the project</returns>
    /// <response code="200">Returns the GitHub contributions</response>
    /// <response code="404">If the project is not found or has no contributions</response>
    [HttpGet("{id:guid}/github-contributions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGithubContributions(Guid id)
    {
        var result = await _projectService.GetProjectGithubContributionsAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
