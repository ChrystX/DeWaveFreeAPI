using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly DeWaveAPIDbContext _dbContext;

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
            var studentId = await GetStudentId();
            if (studentId == null)
                return Unauthorized("Student identity not found in token.");

            try
            {
                await _attendanceService.CheckInAsync(eventId, studentId.Value);
                return Ok(new { message = "Checked in successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for event {EventId}", eventId);
                return StatusCode(500, new { message = "An unexpected message occurred." });
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
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for event {EventId}", eventId);
                return StatusCode(500, new { message = "An unexpected message occurred." });
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
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk marking attendance for event {EventId}", eventId);
                return StatusCode(500, new { message = "An unexpected message occurred." });
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
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance report for event {EventId}", eventId);
                return StatusCode(500, new { message = "An unexpected message occurred." });
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
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance stats for event {EventId}", eventId);
                return StatusCode(500, new { message = "An unexpected message occurred." });
            }
        }

        // GET api/events/{eventId}/attendance/me
        // Student checks their own attendance status
        [HttpGet("me")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> GetMyAttendance(int eventId)
        {
            var studentId = await GetStudentId();
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
                return StatusCode(500, new { message = "An unexpected message occurred." });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private async Task<int?> GetStudentId()
        {
            var userId = User.GetUserId();
            if (userId == null) return null;

            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.UserId == userId.Value);
            return student?.Id;
        }
    }
}