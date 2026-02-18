using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly DeWaveAPIDbContext _dbContext;
        private readonly IMidtransService _midtrans;
        private readonly IEnrollmentService _enrollment;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger,
            DeWaveAPIDbContext dewaveAPIDbContext,
            IMidtransService midtrans,           // ← add
            IEnrollmentService enrollment)
        {
            _paymentService = paymentService;
            _logger = logger;
            _dbContext = dewaveAPIDbContext;
            _midtrans = midtrans;                // ← add
            _enrollment = enrollment;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _paymentService.CreatePaymentAsync(userId.Value, dto.CourseId);
                return Ok(new { token = result.SnapToken, orderId = result.OrderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PaymentGatewayException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }

        [HttpGet("check/{courseId}")]
        public async Task<IActionResult> CheckEnrollment(int courseId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null) return Ok(new { isEnrolled = false });

            var enrolled = await _dbContext.StudentCourses.AnyAsync(e =>
                e.StudentId == student.Id &&
                e.CourseId == courseId &&
                e.IsActive);

            return Ok(new { isEnrolled = enrolled });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllPayments(
    [FromQuery] string? status,
    [FromQuery] DateTime? dateFrom,
    [FromQuery] DateTime? dateTo,
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var query = _dbContext.Payments
                .Include(p => p.Student).ThenInclude(s => s.User)
                .Include(p => p.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(p => p.CreatedAt <= dateTo.Value.AddDays(1));

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p =>
                    p.OrderId.Contains(search) ||
                    p.Student.User.Username.Contains(search) ||
                    p.Course.Title.Contains(search));

            var total = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.Amount,
                    p.Status,
                    p.CreatedAt,
                    StudentName = p.Student.User.Username,
                    CourseName = p.Course.Title,
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data = payments });
        }

        [HttpPost("sync/{orderId}")]
        public async Task<IActionResult> SyncPaymentStatus(string orderId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var payment = await _dbContext.Payments
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.OrderId == orderId
                                       && p.Student.UserId == userId.Value);

            if (payment == null) return NotFound();
            if (payment.Status == "Success") return Ok(new { status = "Success" });

            var midtransStatus = await _midtrans.GetTransactionStatusAsync(orderId);

            payment.Status = midtransStatus switch
            {
                "settlement" or "capture" => "Success",
                "expire" or "cancel" or "deny" => "Failed",
                _ => payment.Status
            };

            await _dbContext.SaveChangesAsync();

            if (payment.Status == "Success")
                await _enrollment.EnrollIfNotAlreadyAsync(
                    payment.StudentId, payment.CourseId, orderId);

            return Ok(new { status = payment.Status });
        }

        [AllowAnonymous]
        [HttpPost("notification")]
        public async Task<IActionResult> Notification([FromBody] JsonElement notification)
        {
            if (!notification.TryGetProperty("order_id", out var orderIdEl) ||
               !notification.TryGetProperty("transaction_status", out var statusEl) ||
               !notification.TryGetProperty("status_code", out var statusCodeEl) ||
               !notification.TryGetProperty("gross_amount", out var grossAmountEl) ||
               !notification.TryGetProperty("signature_key", out var signatureKeyEl))
            {
                _logger.LogWarning("Notification received with missing fields.");
                return BadRequest("Missing required notification fields.");
            }

            try
            {
                await _paymentService.HandleNotificationAsync(
                    orderId: orderIdEl.GetString()!,
                    transactionStatus: statusEl.GetString()!,
                    statusCode: statusCodeEl.GetString()!,
                    grossAmount: grossAmountEl.GetString()!,
                    receivedSignature: signatureKeyEl.GetString()!);

                return Ok();
            }

            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

        }
    }
}
