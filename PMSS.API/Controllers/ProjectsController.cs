using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Project;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProjectFilterParams filterParams)
    {
        var result = await _projectService.GetAllProjectsAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projectService.GetProjectByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _projectService.CreateProjectAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ProjectId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _projectService.UpdateProjectAsync(id, dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("{id}/github-contributions")]
    public async Task<IActionResult> GetGithubContributions(Guid id)
    {
        var result = await _projectService.GetProjectGithubContributionsAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}
