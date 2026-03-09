using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Course;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing course resources
/// </summary>
[ApiController]
[Route("api/v1/courses")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Retrieve a collection of courses with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering and pagination</param>
    /// <returns>A paginated collection of courses</returns>
    /// <response code="200">Returns the list of courses</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterParams filterParams)
    {
        var result = await _courseService.GetAllCoursesAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific course by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the course</param>
    /// <returns>The requested course resource</returns>
    /// <response code="200">Returns the course</response>
    /// <response code="404">If the course is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new course resource
    /// </summary>
    /// <param name="dto">The course creation data</param>
    /// <returns>The newly created course</returns>
    /// <response code="201">Returns the newly created course</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _courseService.CreateCourseAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.CourseId }, result);
    }

    /// <summary>
    /// Replace an existing course resource
    /// </summary>
    /// <param name="id">The unique identifier of the course to update</param>
    /// <param name="dto">The complete course data for replacement</param>
    /// <returns>The updated course resource</returns>
    /// <response code="200">Returns the updated course</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the course is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _courseService.UpdateCourseAsync(id, dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Delete a course resource
    /// </summary>
    /// <param name="id">The unique identifier of the course to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Course deleted successfully</response>
    /// <response code="404">If the course is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _courseService.DeleteCourseAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }
}
