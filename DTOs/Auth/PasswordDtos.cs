namespace DeWaveFreeAPI.DTOs.Auth
{
    public record ForgotPasswordDto(string Email);

    public record ResetPasswordDto(string Token, string NewPassword);
}
