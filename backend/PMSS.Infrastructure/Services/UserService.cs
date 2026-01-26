using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.User;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Utilities;

namespace PMSS.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetAllUsersAsync(UserFilterParams filterParams)
    {
        try
        {
            var query = (await _unitOfWork.Users.GetAllAsync()).AsQueryable();

            if (filterParams.Role.HasValue)
                query = query.Where(u => u.Role == filterParams.Role.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.GithubUsername))
                query = query.Where(u => u.GithubUsername == filterParams.GithubUsername);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(u => u.Name.Contains(filterParams.SearchTerm) || u.Email.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .Select(u => MapToDto(u))
                .ToList();

            var result = new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<UserDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<UserDto>>.ErrorResponse("Error retrieving users", ex.Message);
        }
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<UserDto>.ErrorResponse("User not found");

            return ApiResponse<UserDto>.SuccessResponse(MapToDto(user));
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResponse("Error retrieving user", ex.Message);
        }
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
                return ApiResponse<UserDto>.ErrorResponse("User with this email already exists");

            if (!string.IsNullOrWhiteSpace(dto.GithubUsername))
            {
                var existingUser = await _unitOfWork.Users.GetByGithubUsernameAsync(dto.GithubUsername);
                if (existingUser != null)
                    return ApiResponse<UserDto>.ErrorResponse("User with this GitHub username already exists");
            }

            var user = new User
            {
                Name = dto.Name,
                HashedPassword = PasswordHasher.HashPassword(dto.Password),
                Email = dto.Email,
                GithubUsername = dto.GithubUsername,
                GithubEmail = dto.GithubEmail,
                Role = dto.Role,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResponse(MapToDto(user), "User created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResponse("Error creating user", ex.Message);
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<UserDto>.ErrorResponse("User not found");

            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null && existingUser.UserId != id)
                return ApiResponse<UserDto>.ErrorResponse("User with this email already exists");

            if (!string.IsNullOrWhiteSpace(dto.GithubUsername))
            {
                var existingGithubUser = await _unitOfWork.Users.GetByGithubUsernameAsync(dto.GithubUsername);
                if (existingGithubUser != null && existingGithubUser.UserId != id)
                    return ApiResponse<UserDto>.ErrorResponse("User with this GitHub username already exists");
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.GithubUsername = dto.GithubUsername;
            user.GithubEmail = dto.GithubEmail;
            user.Role = dto.Role;
            user.UpdatedAt = DateTime.Now;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResponse(MapToDto(user), "User updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.ErrorResponse("Error updating user", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found");

            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse("Error deleting user", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found");

            if (!PasswordHasher.VerifyPassword(dto.CurrentPassword, user.HashedPassword))
                return ApiResponse<bool>.ErrorResponse("Current password is incorrect");

            user.HashedPassword = PasswordHasher.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.Now;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Password updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse("Error updating password", ex.Message);
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            GithubUsername = user.GithubUsername,
            GithubEmail = user.GithubEmail,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderBy(u => u.Name);

        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
            "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "role" => descending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "createdat" => descending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.Name)
        };
    }
}
