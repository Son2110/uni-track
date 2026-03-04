using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.ClassEnrollment;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing class enrollment resources.
/// Enrollments are nested under classes following RESTful resource hierarchy.
/// </summary>
[ApiController]
[Produces("application/json")]
[Authorize]
public class ClassEnrollmentsController : ControllerBase
{
    private readonly IClassEnrollmentService _enrollmentService;

    public ClassEnrollmentsController(IClassEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Retrieve all enrollments with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering and pagination</param>
    /// <returns>A paginated collection of enrollments</returns>
    /// <response code="200">Returns the list of enrollments</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet("api/v1/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] ClassEnrollmentFilterParams filterParams)
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve all enrollments for a specific class (nested resource)
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <returns>Collection of enrollments for the class</returns>
    /// <response code="200">Returns the enrollments</response>
    /// <response code="404">If the class is not found</response>
    [HttpGet("api/v1/classes/{classId:guid}/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClassId(Guid classId)
    {
        var result = await _enrollmentService.GetEnrollmentsByClassIdAsync(classId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve enrollment count for a specific class
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <returns>The enrollment count</returns>
    /// <response code="200">Returns the count</response>
    /// <response code="404">If the class is not found</response>
    [HttpGet("api/v1/classes/{classId:guid}/enrollments/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollmentCount(Guid classId)
    {
        var result = await _enrollmentService.GetEnrollmentCountByClassIdAsync(classId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific enrollment for a user in a class
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The enrollment details</returns>
    /// <response code="200">Returns the enrollment</response>
    /// <response code="404">If the enrollment is not found</response>
    [HttpGet("api/v1/classes/{classId:guid}/enrollments/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollment(Guid classId, Guid userId)
    {
        var result = await _enrollmentService.GetEnrollmentAsync(classId, userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve all enrollments for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Collection of enrollments for the user</returns>
    /// <response code="200">Returns the enrollments</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("api/v1/users/{userId:guid}/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _enrollmentService.GetEnrollmentsByUserIdAsync(userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Enroll a student in a class (create enrollment resource)
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="dto">The enrollment creation data</param>
    /// <returns>The newly created enrollment</returns>
    /// <response code="201">Returns the newly created enrollment</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="409">If the student is already enrolled</response>
    [HttpPost("api/v1/classes/{classId:guid}/enrollments")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnrollStudent(Guid classId, [FromBody] CreateClassEnrollmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ensure classId from route matches DTO
        dto.ClassId = classId;

        var result = await _enrollmentService.EnrollStudentAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetEnrollment), 
            new { classId = dto.ClassId, userId = dto.UserId }, 
            result);
    }

    /// <summary>
    /// Bulk enroll multiple students in a class
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="dto">The bulk enrollment data containing user IDs</param>
    /// <returns>Bulk enrollment result</returns>
    /// <response code="200">Returns the bulk enrollment result</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("api/v1/classes/{classId:guid}/enrollments/bulk")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkEnrollStudents(Guid classId, [FromBody] BulkEnrollmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Ensure classId from route is used
        dto.ClassId = classId;

        var result = await _enrollmentService.BulkEnrollStudentsAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Unenroll a student from a class (delete enrollment resource)
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="userId">The unique identifier of the user to unenroll</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Enrollment deleted successfully</response>
    /// <response code="404">If the enrollment is not found</response>
    [HttpDelete("api/v1/classes/{classId:guid}/enrollments/{userId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnenrollStudent(Guid classId, Guid userId)
    {
        var result = await _enrollmentService.UnenrollStudentAsync(classId, userId);

        if (!result.Success)
            return NotFound(result);

        return NoContent();
    }
}
