using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Auth;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DeWaveFreeAPI.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterStudentAsync(RegisterDto request);
        Task VerifyEmailAsync(string token);
        Task SendPasswordResetTokenAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto request);
        Task<UserDto> CreateInstructorAsync(RegisterDto request);
    }

    public class UserService : IUserService
    {

        private readonly DeWaveAPIDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IDisplayIdGenerator _displayIdGenerator;

        public UserService(
            DeWaveAPIDbContext context,
            IEmailService emailService,
            IDisplayIdGenerator displayIdGenerator)
        {
            _context = context;
            _emailService = emailService;
            _displayIdGenerator = displayIdGenerator;
        }

        private async Task<UserDto> MapToUserDto(User user)
        {
            int? instructorId = null;
            if (user.Role.Name == "instructor")
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

        public async Task<UserDto> CreateInstructorAsync(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "instructor");
            if (role == null)
                throw new InvalidOperationException("instructore role not found");

            var displayId = await _displayIdGenerator.GenerateAsync("instructor");

            using var tx = await _context.Database.BeginTransactionAsync();

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = role.Id,
                DisplayId = displayId,
                IsActive = true,
                IsEmailVerified = true, // admin-created
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Instructors.Add(new Instructor
            {
                UserId = user.Id,
                Name = user.Username,
                ContactEmail = user.Email,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return await MapToUserDto(user);
        }

        public async Task<UserDto> RegisterStudentAsync(RegisterDto request)
        {
            // 🔒 Force Student role (ignore request.RoleName)
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "student");
            if (role == null)
                throw new InvalidOperationException("Student role not found");

            // Validation
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists");

            var displayId = await _displayIdGenerator.GenerateAsync("student");

            using var tx = await _context.Database.BeginTransactionAsync();

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

            // Create Student profile
            _context.Students.Add(new Student
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // Send email verification
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendVerificationEmailAsync(
                    user.Email,
                    user.EmailVerificationToken
                );
            }

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return await MapToUserDto(user);
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

        public async Task SendPasswordResetTokenAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return; // security best practice

            user.PasswordResetToken = GenerateSecureToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(
                email,
                user.PasswordResetToken
            );
        }


        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == request.Token &&
                    u.PasswordResetTokenExpires > DateTime.UtcNow
                );

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


    }
}
