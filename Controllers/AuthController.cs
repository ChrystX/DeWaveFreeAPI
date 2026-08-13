using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.DTOs.Auth;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            try
            {
                var result = await _userService.RegisterStudentAsync(request);
                return Ok(new
                {
                    message = "Registration successful",
                    user = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("admin/create-instructor")]
        public async Task<IActionResult> CreateInstructor([FromBody] RegisterDto request)
        {
            try
            {
                var result = await _userService.CreateInstructorAsync(request);

                return Ok(new
                {
                    message = "Lecturer created successfully",
                    user = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create lecturer failed");
                return StatusCode(500, new { message = "An error occurred while creating lecturer" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.GetUserId()?.ToString();
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var displayId = User.FindFirst("displayId")?.Value;

            return Ok(new
            {
                id = userId,
                username = username,
                role = role,
                displayId = displayId 
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            // If we reach here, the token is valid (middleware already validated it)
            var userId = User.GetUserId()?.ToString();
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var expirationClaim = User.FindFirst("exp")?.Value;

            DateTime? expiresAt = null;
            if (long.TryParse(expirationClaim, out long exp))
            {
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            }

            return Ok(new
            {
                valid = true,
                user = new
                {
                    id = userId,
                    username = username,
                    role = role
                },
                expiresAt = expiresAt
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                await _authService.SendPasswordResetTokenAsync(request.Email);
                return Ok(new { message = "If the email exists, a reset link was sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed");
                return StatusCode(500, new { message = "Error processing request" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);
                return Ok(new { message = "Password reset successful" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed");
                return StatusCode(500, new { message = "Error resetting password" });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            try
            {
                await _authService.VerifyEmailAsync(request.Token);
                return Ok(new { message = "Email verified successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification failed");
                return StatusCode(500, new { message = "Error verifying email" });
            }
        }
    }
}