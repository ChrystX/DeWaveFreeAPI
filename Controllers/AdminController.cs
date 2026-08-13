using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Services;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly IDisplayIdGenerator _displayIdGenerator;
        private readonly IAuthService _authService;

        public AdminController(DeWaveAPIDbContext context, IDisplayIdGenerator displayIdGenerator, IAuthService authService)
        {
            _context = context;
            _displayIdGenerator = displayIdGenerator;
            _authService = authService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayId,
                    u.Username,
                    u.Email,
                    Role = u.Role.Name,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayId,
                    u.Username,
                    u.Email,
                    RoleId = u.RoleId,
                    Role = u.Role.Name,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                return BadRequest(ModelState);
            }

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return BadRequest("Invalid role");

            var displayId = await _displayIdGenerator.GenerateAsync(role.Name);
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                RoleId = dto.RoleId,
                DisplayId = displayId,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (role.Name.Equals("instructor", StringComparison.OrdinalIgnoreCase))
            {
                var instructor = new Instructor
                {
                    UserId = user.Id,
                    Name = user.Username,
                    ContactEmail = user.Email,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
            }

            try
            {
                await _authService.SendAccountSetupEmailAsync(user.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send setup email: {ex.Message}");
            }

            return Ok(user.Id);
        }

        [HttpPost("users/{id}/resend-setup")]
        public async Task<IActionResult> ResendSetupEmail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            await _authService.SendAccountSetupEmailAsync(user.Email);
            return Ok(new { message = "Setup email resent" });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.RoleId = dto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, ChangeRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.RoleId = dto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}