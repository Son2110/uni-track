using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.ProjectMember;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    /// <summary>
    /// Get all project members with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Filter parameters including ProjectId, UserId, PageNumber, PageSize</param>
    /// <returns>Paginated list of project members</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProjectMemberFilterParams filterParams)
    {
        var result = await _projectMemberService.GetAllMembersAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Get a specific membership by project and user ID
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The membership details</returns>
    [HttpGet("{projectId}/{userId}")]
    public async Task<IActionResult> GetMembership(Guid projectId, Guid userId)
    {
        var result = await _projectMemberService.GetMembershipAsync(projectId, userId);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Get all members of a specific project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of project members</returns>
    [HttpGet("project/{projectId}")]
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
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Get all projects a user is a member of
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of project memberships</returns>
    [HttpGet("user/{userId}")]
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
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Add a member to a project
    /// </summary>
    /// <param name="dto">Create project member DTO containing ProjectId and UserId</param>
    /// <returns>The created membership</returns>
    [HttpPost]
    public async Task<IActionResult> AddMember([FromBody] CreateProjectMemberDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _projectMemberService.AddMemberAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(
            nameof(GetMembership), 
            new { projectId = result.Data!.ProjectId, userId = result.Data.UserId }, 
            result);
    }

    /// <summary>
    /// Remove a member from a project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{projectId}/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
    {
        var result = await _projectMemberService.RemoveMemberAsync(projectId, userId);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}
