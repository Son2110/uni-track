using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.ClassEnrollment;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassEnrollmentsController : ControllerBase
{
    private readonly IClassEnrollmentService _enrollmentService;

    public ClassEnrollmentsController(IClassEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ClassEnrollmentFilterParams filterParams)
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("class/{classId}/user/{userId}")]
    public async Task<IActionResult> GetEnrollment(Guid classId, Guid userId)
    {
        var result = await _enrollmentService.GetEnrollmentAsync(classId, userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("class/{classId}")]
    public async Task<IActionResult> GetByClassId(Guid classId)
    {
        var result = await _enrollmentService.GetEnrollmentsByClassIdAsync(classId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("class/{classId}/count")]
    public async Task<IActionResult> GetEnrollmentCount(Guid classId)
    {
        var result = await _enrollmentService.GetEnrollmentCountByClassIdAsync(classId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _enrollmentService.GetEnrollmentsByUserIdAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent([FromBody] CreateClassEnrollmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _enrollmentService.EnrollStudentAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetEnrollment), 
            new { classId = dto.ClassId, userId = dto.UserId }, result);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkEnrollStudents([FromBody] BulkEnrollmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _enrollmentService.BulkEnrollStudentsAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("class/{classId}/user/{userId}")]
    public async Task<IActionResult> UnenrollStudent(Guid classId, Guid userId)
    {
        var result = await _enrollmentService.UnenrollStudentAsync(classId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
