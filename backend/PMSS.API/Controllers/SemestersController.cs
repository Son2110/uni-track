using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Semester;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing semester resources
/// </summary>
[ApiController]
[Route("api/v1/semesters")]
[Produces("application/json")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    /// <summary>
    /// Retrieve a collection of semesters with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering and pagination</param>
    /// <returns>A paginated collection of semesters</returns>
    /// <response code="200">Returns the list of semesters</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] SemesterFilterParams filterParams)
    {
        var result = await _semesterService.GetAllSemestersAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific semester by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the semester</param>
    /// <returns>The requested semester resource</returns>
    /// <response code="200">Returns the semester</response>
    /// <response code="404">If the semester is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _semesterService.GetSemesterByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new semester resource
    /// </summary>
    /// <param name="dto">The semester creation data</param>
    /// <returns>The newly created semester</returns>
    /// <response code="201">Returns the newly created semester</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _semesterService.CreateSemesterAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.SemesterId }, result);
    }

    /// <summary>
    /// Replace an existing semester resource
    /// </summary>
    /// <param name="id">The unique identifier of the semester to update</param>
    /// <param name="dto">The complete semester data for replacement</param>
    /// <returns>The updated semester resource</returns>
    /// <response code="200">Returns the updated semester</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the semester is not found</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSemesterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _semesterService.UpdateSemesterAsync(id, dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Delete a semester resource
    /// </summary>
    /// <param name="id">The unique identifier of the semester to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Semester deleted successfully</response>
    /// <response code="404">If the semester is not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _semesterService.DeleteSemesterAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }
}
