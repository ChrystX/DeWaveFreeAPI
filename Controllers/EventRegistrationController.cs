using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Extension;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventRegistrationController : ControllerBase
    {
        private readonly IEventRegistrationService _registrationService;
        private readonly IEventAccessService _accessService;
        private readonly DeWaveAPIDbContext _dbContext;
        private readonly ILogger<EventRegistrationController> _logger;

        public EventRegistrationController(
            IEventRegistrationService registrationService,
            IEventAccessService accessService,
            DeWaveAPIDbContext dbContext,
            ILogger<EventRegistrationController> logger)
        {
            _registrationService = registrationService;
            _accessService = accessService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("{eventId}/register")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Register(int eventId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound(new { message = "Student profile not found." });

            try
            {
                await _registrationService.RegisterForEventAsync(eventId, student.Id);
                return Ok(new { message = "Successfully registered for event." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register student {StudentId} for event {EventId}", student.Id, eventId);
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        [HttpDelete("{eventId}/register")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> CancelRegistration(int eventId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound(new { message = "Student profile not found." });

            try
            {
                var cancelled = await _registrationService.CancelRegistrationAsync(eventId, student.Id);
                return cancelled ? Ok(new { message = "Registration cancelled." }) : NotFound(new { message = "No active registration found." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel registration for student {StudentId} on event {EventId}", student.Id, eventId);
                return StatusCode(500, new { message = "An error occurred while cancelling registration." });
            }
        }

        [HttpGet("{eventId}/register")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> CheckRegistration(int eventId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound(new { message = "Student profile not found." });

            var isRegistered = await _registrationService.IsStudentRegisteredAsync(eventId, student.Id);
            var canRegister = await _registrationService.CanRegisterAsync(eventId, student.Id);
            var availableSpots = await _registrationService.GetAvailableSpotsAsync(eventId);

            return Ok(new
            {
                isRegistered,
                canRegister,
                availableSpots = availableSpots == int.MaxValue ? (int?)null : availableSpots
            });
        }

        [HttpGet("{eventId}/registrations")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> GetRegistrations(int eventId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var courseEvent = await _dbContext.CourseEvents.FirstOrDefaultAsync(e => e.Id == eventId);
            if (courseEvent == null) return NotFound();

            try
            {
                // Only the event's owning instructor (or an admin) may view its roster.
                await _accessService.EnsureEventOwnerOrAdminAsync(courseEvent, userId.Value);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            try
            {
                var registrations = await _registrationService.GetEventRegistrationsAsync(eventId);
                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get registrations for event {EventId}", eventId);
                return StatusCode(500, new { message = "An error occurred while fetching registrations." });
            }
        }

        private async Task<Student?> GetStudentAsync()
        {
            var userId = User.GetUserId();
            if (userId == null) return null;
            return await _dbContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        }
    }
}
