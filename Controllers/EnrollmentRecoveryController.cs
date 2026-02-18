using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "admin")]
    public class EnrollmentRecoveryController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;
        private readonly IEnrollmentService _enrollment;
        private readonly ILogger<EnrollmentRecoveryController> _logger;

        public EnrollmentRecoveryController(
            DeWaveAPIDbContext dbContext,
            IEnrollmentService enrollment,
            ILogger<EnrollmentRecoveryController> logger)
        {
            _dbContext = dbContext;
            _enrollment = enrollment;
            _logger = logger;
        }

        /// <summary>
        /// Returns all payments with Status = "Success" where the student
        /// is NOT yet enrolled in the course. These are the broken cases.
        /// </summary>
        [HttpGet("missing-enrollments")]
        public async Task<IActionResult> GetMissingEnrollments()
        {
            var broken = await _dbContext.Payments
                .Where(p => p.Status == "Success")
                .Where(p => !_dbContext.StudentCourses.Any(sc =>
                    sc.StudentId == p.StudentId &&
                    sc.CourseId == p.CourseId))
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .Include(p => p.Course)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.Amount,
                    p.CreatedAt,
                    p.Status,
                    Student = new
                    {
                        p.Student.Id,
                        p.Student.User.Username,
                        p.Student.User.Email
                    },
                    Course = new
                    {
                        p.Course.Id,
                        p.Course.Title
                    }
                })
                .ToListAsync();

            return Ok(broken);
        }

        /// <summary>
        /// Manually triggers enrollment for a specific payment by orderId.
        /// Guards: payment must exist AND be Success. Idempotent — safe to call
        /// multiple times thanks to EnrollIfNotAlreadyAsync.
        /// </summary>
        [HttpPost("recover/{orderId}")]
        public async Task<IActionResult> RecoverEnrollment(string orderId)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                return NotFound($"No payment found for order '{orderId}'.");

            if (payment.Status != "Success")
                return BadRequest(
                    $"Payment '{orderId}' has status '{payment.Status}'. " +
                    "Only successful payments can be manually enrolled.");

            var alreadyEnrolled = await _dbContext.StudentCourses.AnyAsync(sc =>
                sc.StudentId == payment.StudentId &&
                sc.CourseId == payment.CourseId);

            if (alreadyEnrolled)
                return Ok(new { message = "Student is already enrolled. Nothing to recover." });

            await _enrollment.EnrollIfNotAlreadyAsync(
                payment.StudentId,
                payment.CourseId,
                orderId);

            _logger.LogInformation(
                "Admin manually recovered enrollment for order {OrderId} " +
                "(StudentId={StudentId}, CourseId={CourseId}).",
                orderId, payment.StudentId, payment.CourseId);

            return Ok(new
            {
                message = "Enrollment recovered successfully.",
                orderId,
                payment.StudentId,
                payment.CourseId
            });
        }
    }
}