using PMSS.Application.DTOs.Auth;
using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
}
