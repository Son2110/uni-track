using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.ProjectMember;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing project member resources.
/// Members are nested under projects following RESTful resource hierarchy.
/// </summary>
[ApiController]
[Produces("application/json")]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    /// <summary>
    /// Retrieve all project members with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Filter parameters including ProjectId, UserId, PageNumber, PageSize</param>
    /// <returns>Paginated list of project members</returns>
    /// <response code="200">Returns the list of members</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet("api/v1/project-members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] ProjectMemberFilterParams filterParams)
    {
        var result = await _projectMemberService.GetAllMembersAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve all members of a specific project (nested resource)
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of project members</returns>
    /// <response code="200">Returns the list of members</response>
    /// <response code="404">If the project is not found</response>
    [HttpGet("api/v1/projects/{projectId:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectMembers(
        Guid projectId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        var filterParams = new ProjectMemberFilterParams
        {
            ProjectId = projectId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _projectMemberService.GetAllMembersAsync(filterParams);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific membership by project and user ID
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The membership details</returns>
    /// <response code="200">Returns the membership</response>
    /// <response code="404">If the membership is not found</response>
    [HttpGet("api/v1/projects/{projectId:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembership(Guid projectId, Guid userId)
    {
        var result = await _projectMemberService.GetMembershipAsync(projectId, userId);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve all projects a user is a member of
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of project memberships</returns>
    /// <response code="200">Returns the list of memberships</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("api/v1/users/{userId:guid}/memberships")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProjects(
        Guid userId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        var filterParams = new ProjectMemberFilterParams
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _projectMemberService.GetAllMembersAsync(filterParams);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Add a member to a project (create membership resource)
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="dto">Create project member DTO containing UserId</param>
    /// <returns>The created membership</returns>
    /// <response code="201">Returns the newly created membership</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="409">If the user is already a member</response>
    [HttpPost("api/v1/projects/{projectId:guid}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(Guid projectId, [FromBody] CreateProjectMemberDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ensure projectId from route is used
        dto.ProjectId = projectId;

        var result = await _projectMemberService.AddMemberAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(
            nameof(GetMembership), 
            new { projectId = result.Data!.ProjectId, userId = result.Data.UserId }, 
            result);
    }

    /// <summary>
    /// Remove a member from a project (delete membership resource)
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Membership deleted successfully</response>
    /// <response code="404">If the membership is not found</response>
    [HttpDelete("api/v1/projects/{projectId:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
    {
        var result = await _projectMemberService.RemoveMemberAsync(projectId, userId);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }
}
