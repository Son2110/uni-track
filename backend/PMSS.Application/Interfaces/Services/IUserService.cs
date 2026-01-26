using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.User;

namespace PMSS.Application.Interfaces.Services;

public interface IUserService
{
    Task<ApiResponse<PagedResult<UserDto>>> GetAllUsersAsync(UserFilterParams filterParams);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    Task<ApiResponse<bool>> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto);
}
