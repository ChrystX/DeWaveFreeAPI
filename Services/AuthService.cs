using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
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
        Task<UserDto> RegisterAsync(RegisterDto request);
        Task<LoginResponseDto> LoginAsync(LoginDto request);
        Task VerifyEmailAsync(string token);
        Task SendPasswordResetTokenAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto request);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly DeWaveAPIDbContext _context; // Replace with your actual DbContext name
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IDisplayIdGenerator _displayIdGenerator;

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto(
                user.Id,
                user.Username,
                user.Email ?? "",
                user.DisplayId,
                user.Role.Name,
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt,
                user.LastLoginAt
            );
        }

        public AuthService(DeWaveAPIDbContext context, IOptions<JwtSettings> jwtSettings, IEmailService emailService, IDisplayIdGenerator displayIdGenerator)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _displayIdGenerator = displayIdGenerator;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto request)
        {
            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists");

            // Check if email exists
            if (!string.IsNullOrEmpty(request.Email) &&
                await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists");

            // Get role
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
            if (role == null)
                throw new InvalidOperationException("Invalid role");

            // Generate display ID
            var displayId = await _displayIdGenerator.GenerateAsync(request.RoleName);

            // Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = role.Id,
                DisplayId = displayId,
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationToken = GenerateSecureToken(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send verification email
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken);
            }

            // Load role for mapping
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return MapToUserDto(user);
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // TODO: Implement refresh token logic
            // This requires:
            // 1. Creating a RefreshToken table/model to store tokens
            // 2. Validating the refresh token
            // 3. Generating new access token
            // 4. Rotating refresh token (optional but recommended)

            throw new NotImplementedException("Refresh token functionality not yet implemented");
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
            var refreshToken = GenerateSecureToken();

            return new LoginResponseDto(
                accessToken,
                refreshToken,
                MapToUserDto(user)
            );

        }

        public async Task VerifyEmailAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
                throw new InvalidOperationException("Invalid verification token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
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

        public async Task SendPasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return; // Don't reveal if email exists (security best practice)

            user.PasswordResetToken = GenerateSecureToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(email, user.PasswordResetToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token
                    && u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                throw new InvalidOperationException("Invalid or expired reset token");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
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
    }
}