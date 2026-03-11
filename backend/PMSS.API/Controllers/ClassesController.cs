using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Class;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing class resources
/// </summary>
[ApiController]
[Route("api/v1/classes")]
[Produces("application/json")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    /// <summary>
    /// Retrieve a collection of classes with optional filtering and pagination.
    /// Supports filtering by teacherId, semesterId, and courseId via query parameters.
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering (teacherId, semesterId, courseId) and pagination</param>
    /// <returns>A paginated collection of classes</returns>
    /// <response code="200">Returns the list of classes</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] ClassFilterParams filterParams)
    {
        var result = await _classService.GetAllClassesAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific class by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the class</param>
    /// <returns>The requested class resource</returns>
    /// <response code="200">Returns the class</response>
    /// <response code="404">If the class is not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _classService.GetClassByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new class resource
    /// </summary>
    /// <param name="dto">The class creation data</param>
    /// <returns>The newly created class</returns>
    /// <response code="201">Returns the newly created class</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _classService.CreateClassAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ClassId }, result);
    }

    /// <summary>
    /// Replace an existing class resource
    /// </summary>
    /// <param name="id">The unique identifier of the class to update</param>
    /// <param name="dto">The complete class data for replacement</param>
    /// <returns>The updated class resource</returns>
    /// <response code="200">Returns the updated class</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the class is not found</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _classService.UpdateClassAsync(id, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a class resource
    /// </summary>
    /// <param name="id">The unique identifier of the class to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Class deleted successfully</response>
    /// <response code="404">If the class is not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _classService.DeleteClassAsync(id);

        if (!result.Success)
            return NotFound(result);

        return NoContent();
    }
}
