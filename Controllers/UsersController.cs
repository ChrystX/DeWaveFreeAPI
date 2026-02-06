using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly DeWaveAPIDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(DeWaveAPIDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var userDto = new UserDto(
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

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update email if provided and different
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
                    return BadRequest(new { message = "Email already in use" });

                user.Email = request.Email;
                user.IsEmailVerified = false; // Require re-verification if email changes
            }

            // Update username if provided and different
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != userId))
                    return BadRequest(new { message = "Username already in use" });

                user.Username = request.Username;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect" });

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{displayId}")]
    public async Task<IActionResult> GetUserByDisplayId(string displayId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.DisplayId == displayId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var userDto = new UserDto(
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

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // Admin only endpoint
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Users.Include(u => u.Role);

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto(
                    u.Id,
                    u.Username,
                    u.Email ?? "",
                    u.DisplayId,
                    u.Role.Name,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    u.LastLoginAt
                ))
                .ToListAsync();

            return Ok(new
            {
                users,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}