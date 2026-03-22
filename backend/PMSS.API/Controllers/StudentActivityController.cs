using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// Controller for monitoring student coding activity and notifying teachers
/// about the least active students in their classes.
/// </summary>
[ApiController]
[Route("api/v1/student-activity")]
[Produces("application/json")]
[Authorize]
public class StudentActivityController : ControllerBase
{
    private readonly IStudentActivityMonitorService _monitorService;

    public StudentActivityController(IStudentActivityMonitorService monitorService)
    {
        _monitorService = monitorService;
    }

    /// <summary>
    /// Manually trigger student activity check for all classes.
    /// Sends notifications to each teacher with the top 10 least active students per class.
    /// </summary>
    /// <param name="recentWeeks">Number of recent weeks to analyze (default: 4)</param>
    /// <returns>Summary of the activity check results</returns>
    /// <response code="200">Activity check completed successfully</response>
    /// <response code="500">If an error occurred during the check</response>
    [HttpPost("check-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckAllStudentActivity([FromQuery] int recentWeeks = 4)
    {
        var result = await _monitorService.CheckAndNotifyAllAsync(recentWeeks);

        if (!result.Success)
            return StatusCode(StatusCodes.Status500InternalServerError, result);

        return Ok(result);
    }

    /// <summary>
    /// Manually trigger student activity check for a specific class.
    /// Sends a notification to the teacher with the top 10 least active students.
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="recentWeeks">Number of recent weeks to analyze (default: 4)</param>
    /// <returns>Activity report for the class</returns>
    /// <response code="200">Activity check completed successfully</response>
    /// <response code="404">If the class is not found</response>
    /// <response code="500">If an error occurred during the check</response>
    [HttpPost("check-class/{classId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckClassStudentActivity(Guid classId, [FromQuery] int recentWeeks = 4)
    {
        var result = await _monitorService.CheckAndNotifyByClassAsync(classId, recentWeeks);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get student activity reports for all classes taught by a specific teacher.
    /// Returns the top 10 least active students per class without sending notifications.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="recentWeeks">Number of recent weeks to analyze (default: 4)</param>
    /// <returns>Activity reports grouped by class</returns>
    /// <response code="200">Activity data retrieved successfully</response>
    /// <response code="404">If the teacher is not found</response>
    /// <response code="500">If an error occurred during the retrieval</response>
    [HttpGet("teachers/{teacherId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByTeacher(Guid teacherId, [FromQuery] int recentWeeks = 4)
    {
        var result = await _monitorService.GetActivityByTeacherAsync(teacherId, recentWeeks);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(result);
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }
}
