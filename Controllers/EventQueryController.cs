using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventQueryController : ControllerBase
    {
        private readonly IEventQueryService _queryService;
        private readonly DeWaveAPIDbContext _dbContext;
        private readonly ILogger<EventQueryController> _logger;

        public EventQueryController(
            IEventQueryService queryService,
            DeWaveAPIDbContext dbContext,
            ILogger<EventQueryController> logger)
        {
            _queryService = queryService;
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET api/events
        // Student  → calendar feed (filtered by enrolled courses)
        // Instructor/Admin → table feed (their own events)
        // Unauthenticated → public events only
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents([FromQuery] EventFilterDto filter)
        {
            var userId = User.GetUserId();
            var role = User.FindFirstValue(ClaimTypes.Role);

            try
            {
                var events = await _queryService.GetEventsAsync(filter, userId, role);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get events for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while fetching events." });
            }
        }

        // GET api/events/{id}
        // General event detail — used for calendar popup / modal
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventDetail(int id)
        {
            try
            {
                var detail = await _queryService.GetEventDetailAsync(id);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get event detail for event {EventId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching event detail." });
            }
        }

        // GET api/events/{id}/student-detail
        // Student-specific detail — registration status, can register, meeting url gated
        [HttpGet("{id}/student-detail")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> GetStudentEventDetail(int id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var student = await GetStudentAsync();
            if (student == null) return NotFound(new { message = "Student profile not found." });

            try
            {
                var detail = await _queryService.GetStudentEventDetailAsync(id, student.Id);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get student event detail for event {EventId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching event detail." });
            }
        }

        // GET api/events/{id}/instructor-detail
        // Instructor/Admin detail — registered count, attended count, capacity
        [HttpGet("{id}/instructor-detail")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> GetInstructorEventDetail(int id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var detail = await _queryService.GetInstructorEventDetailAsync(id, userId.Value);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get instructor event detail for event {EventId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching event detail." });
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