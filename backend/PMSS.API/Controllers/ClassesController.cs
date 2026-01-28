using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Class;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ClassFilterParams filterParams)
    {
        var result = await _classService.GetAllClassesAsync(filterParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _classService.GetClassByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<IActionResult> GetByTeacherId(Guid teacherId)
    {
        var result = await _classService.GetClassesByTeacherIdAsync(teacherId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("semester/{semesterId}")]
    public async Task<IActionResult> GetBySemesterId(Guid semesterId)
    {
        var result = await _classService.GetClassesBySemesterIdAsync(semesterId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetByCourseId(Guid courseId)
    {
        var result = await _classService.GetClassesByCourseIdAsync(courseId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _classService.CreateClassAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ClassId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _classService.UpdateClassAsync(id, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _classService.DeleteClassAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
