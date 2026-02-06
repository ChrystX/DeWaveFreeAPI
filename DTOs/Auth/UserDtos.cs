namespace DeWaveFreeAPI.DTOs.Auth
{
    public record RegisterDto(
        string Username,
        string Password,
        string Email,
        string RoleName = "Student"
    );

    public record LoginDto(
        string Username,
        string Password
    );

    public record RefreshTokenDto(string RefreshToken);

    public record VerifyEmailDto(string Token);

}