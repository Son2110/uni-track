using PMSS.Domain.Enums;

namespace PMSS.Application.DTOs.User;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GithubUsername { get; set; }
    public string? GithubEmail { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GithubUsername { get; set; }
    public string? GithubEmail { get; set; }
    public UserRole Role { get; set; }
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GithubUsername { get; set; }
    public string? GithubEmail { get; set; }
    public UserRole Role { get; set; }
}

public class UpdatePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UserFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public UserRole? Role { get; set; }
    public string? GithubUsername { get; set; }
}
