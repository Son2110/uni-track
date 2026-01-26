using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Course;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterParams filterParams)
    {
        var result = await _courseService.GetAllCoursesAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _courseService.CreateCourseAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.CourseId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _courseService.UpdateCourseAsync(id, dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _courseService.DeleteCourseAsync(id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}
