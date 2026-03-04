using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Auth;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for authentication
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate a user and return a JWT token
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Returns the JWT token</response>
    /// <response code="401">If the credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}
