namespace DeWaveFreeAPI.DTOs.Auth
{
    public record UserDto(
        int Id,
        string Username,
        string Email, string DisplayId,
        string RoleName,
        bool IsActive,
        bool IsEmailVerified,
        DateTime CreatedAt,
        DateTime? LastLoginAt,
        int? InstructorId
    );
    public record UpdateProfileDto(
        string? Email,
        string? Username
    );
    public record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword
    );
}
