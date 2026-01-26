using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Semester;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SemesterFilterParams filterParams)
    {
        var result = await _semesterService.GetAllSemestersAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _semesterService.GetSemesterByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSemesterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _semesterService.CreateSemesterAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.SemesterId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSemesterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _semesterService.UpdateSemesterAsync(id, dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _semesterService.DeleteSemesterAsync(id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}
