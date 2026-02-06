namespace DeWaveFreeAPI.DTOs.Auth
{
    public record LoginResponseDto(
        string AccessToken,
        string RefreshToken,
        UserDto User
    );
}