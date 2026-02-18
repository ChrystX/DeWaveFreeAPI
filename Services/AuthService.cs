using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Auth;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DeWaveFreeAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto request);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task SendPasswordResetTokenAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task VerifyEmailAsync(string token);
        Task SendAccountSetupEmailAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly DeWaveAPIDbContext _context; // Replace with your actual DbContext name
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IDisplayIdGenerator _displayIdGenerator;

        private async Task<UserDto> MapToUserDto(User user)
        {
            int? instructorId = null;
            if (user.Role.Name == "Instructor")
            {
                var instructor = await _context.Instructors
                    .FirstOrDefaultAsync(i => i.UserId == user.Id);
                instructorId = instructor?.Id;
            }

            return new UserDto(
                user.Id,
                user.Username,
                user.Email ?? "",
                user.DisplayId,
                user.Role.Name,
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt,
                user.LastLoginAt,
                instructorId
            );
        }

        public AuthService(DeWaveAPIDbContext context, IOptions<JwtSettings> jwtSettings, IEmailService emailService, IDisplayIdGenerator displayIdGenerator)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _displayIdGenerator = displayIdGenerator;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (existingToken == null)
                throw new UnauthorizedAccessException("Invalid refresh token");

            if (existingToken.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired");

            if (existingToken.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token already used");

            // Atomic check-and-revoke — only one concurrent request will get rowsAffected = 1
            var rowsAffected = await _context.RefreshTokens
                .Where(rt => rt.Token == token && !rt.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true));

            // Another request already won the race — return 401 cleanly instead of crashing
            if (rowsAffected == 0)
                throw new UnauthorizedAccessException("Refresh token already used");

            var newRefreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = existingToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshToken);
            var newAccessToken = GenerateJwtToken(existingToken.User);
            await _context.SaveChangesAsync();

            return new LoginResponseDto(
                newAccessToken,
                newRefreshToken.Token,
                await MapToUserDto(existingToken.User)
            );
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid username or password");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = GenerateJwtToken(user);
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponseDto(
                accessToken,
                refreshToken.Token,
                await MapToUserDto(user)
            );

        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("displayId", user.DisplayId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (existing != null)
            {
                existing.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        private static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendPasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var token = GenerateSecureToken();
            var encodedToken = Uri.EscapeDataString(token);
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, encodedToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == request.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                throw new InvalidOperationException("Invalid or expired token");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();
        }

        public async Task VerifyEmailAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
                throw new InvalidOperationException("Invalid token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;

            await _context.SaveChangesAsync();
        }

        public async Task SendAccountSetupEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var token = GenerateSecureToken();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailService.SendAccountSetupEmailAsync(user.Email, token);
        }
    }
}