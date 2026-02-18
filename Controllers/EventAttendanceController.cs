using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Services;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [Route("api/events/{eventId:int}/attendance")]
    [ApiController]
    [Authorize]
    public class EventAttendanceController : ControllerBase
    {
        private readonly IEventAttendanceService _attendanceService;
        private readonly ILogger<EventAttendanceController> _logger;

        public EventAttendanceController(IEventAttendanceService attendanceService,
            ILogger<EventAttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // POST api/events/{eventId}/attendance/checkin
        // Student self check-in during the event window
        [HttpPost("checkin")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> CheckIn(int eventId)
        {
            var studentId = GetStudentId();
            if (studentId == null)
                return Unauthorized("Student identity not found in token.");

            try
            {
                await _attendanceService.CheckInAsync(eventId, studentId.Value);
                return Ok(new { message = "Checked in successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for event {EventId}", eventId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST api/events/{eventId}/attendance/mark
        // Instructor manually marks a single student
        [HttpPost("mark")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> MarkAttendance(int eventId, [FromBody] MarkAttendanceDto dto)
        {
            dto.EventId = eventId; // enforce route param over body
            try
            {
                await _attendanceService.MarkAttendanceAsync(dto);
                return Ok(new { message = "Attendance marked successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for event {EventId}", eventId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // POST api/events/{eventId}/attendance/bulk-mark
        // Instructor marks multiple students at once
        [HttpPost("bulk-mark")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> BulkMarkAttendance(int eventId, [FromBody] BulkMarkAttendanceDto dto)
        {
            dto.EventId = eventId;
            try
            {
                await _attendanceService.BulkMarkAttendanceAsync(dto);
                return Ok(new { message = $"Bulk attendance marked for {dto.Attendances.Count} student(s)." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk marking attendance for event {EventId}", eventId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET api/events/{eventId}/attendance/report
        // Full attendance report — instructor only
        [HttpGet("report")]
        [Authorize(Roles = "instructor")]
        public async Task<IActionResult> GetAttendanceReport(int eventId)
        {
            try
            {
                var report = await _attendanceService.GetAttendanceReportAsync(eventId);
                return Ok(report);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance report for event {EventId}", eventId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET api/events/{eventId}/attendance/stats
        // Lightweight stats — both roles can see
        [HttpGet("stats")]
        [Authorize(Roles = "instructor,student")]
        public async Task<IActionResult> GetAttendanceStats(int eventId)
        {
            try
            {
                var stats = await _attendanceService.GetAttendanceStatsAsync(eventId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance stats for event {EventId}", eventId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // GET api/events/{eventId}/attendance/me
        // Student checks their own attendance status
        [HttpGet("me")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> GetMyAttendance(int eventId)
        {
            var studentId = GetStudentId();
            if (studentId == null)
                return Unauthorized("Student identity not found in token.");

            try
            {
                var attended = await _attendanceService.HasStudentAttendedAsync(eventId, studentId.Value);
                return Ok(new { eventId, studentId, attended });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking attendance for student {StudentId}", studentId);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private int? GetStudentId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)   // or your custom claim key
                     ?? User.FindFirstValue("studentId");

            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}