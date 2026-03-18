using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMSS.Application.DTOs.Auth;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Infrastructure.Configuration;
using PMSS.Infrastructure.Utilities;

namespace PMSS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for email: {Email}", dto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
            }

            if (!PasswordHasher.VerifyPassword(dto.Password, user.HashedPassword))
            {
                _logger.LogWarning("Login failed: invalid password for email: {Email}", dto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var response = new AuthResponseDto
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            _logger.LogInformation("Login successful for user: {UserId}", user.UserId);
            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", dto.Email);
            return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during login", ex.Message);
        }
    }
}
