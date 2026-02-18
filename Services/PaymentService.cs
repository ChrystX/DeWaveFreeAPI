using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{

    public record CreatePaymentResult(string SnapToken, string OrderId);

    public interface IPaymentService
    {
        Task<CreatePaymentResult> CreatePaymentAsync(int userId, int courseId);
        Task HandleNotificationAsync(string orderId, string transactionStatus, string statusCode, string grossAmount, string receivedSignature);
    }
    public class PaymentService : IPaymentService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly IMidtransService _midtrans;
        private readonly IEnrollmentService _enrollment;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            DeWaveAPIDbContext context,
            IMidtransService midtrans,
            IEnrollmentService enrollment,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _midtrans = midtrans;
            _enrollment = enrollment;
            _logger = logger;
        }

        private async Task ExpireAllPendingAsync(int studentId, int courseId)
        {
            var pending = await _context.Payments
                .Where(p =>
                    p.StudentId == studentId &&
                    p.CourseId == courseId &&
                    p.Status == "Pending")
                .ToListAsync();

            foreach (var p in pending)
                p.Status = "Expired";

            await _context.SaveChangesAsync();
        }

        public async Task<CreatePaymentResult> CreatePaymentAsync(int userId, int courseId)
        {
            // 1. Load student + user
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId)
                ?? throw new InvalidOperationException("Student not found.");

            // 2. Load course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId)
                ?? throw new InvalidOperationException("Course not found.");

            // 3. Block already-succeeded purchases
            var alreadyPurchased = await _context.Payments.AnyAsync(p =>
                p.StudentId == student.Id &&
                p.CourseId == course.Id &&
                p.Status == "Success");

            if (alreadyPurchased)
                throw new InvalidOperationException("You have already purchased this course.");

            // 4. Expire stale pending payments (older than 24h)
            await ExpireAllPendingAsync(student.Id, course.Id);

            // 5. Reuse existing pending payment
            var pendingPayment = await _context.Payments
                .Where(p =>
                    p.StudentId == student.Id &&
                    p.CourseId == course.Id &&
                    p.Status == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pendingPayment != null)
            {
                return new CreatePaymentResult(
                    pendingPayment.SnapToken,
                    pendingPayment.OrderId);
            }

            // 6. Get snap token from Midtrans
            var orderId = $"ORDER-{Guid.NewGuid()}";

            var midtransResult = await _midtrans.CreateSnapTokenAsync(
                orderId,
                course.Price,
                student.User.Username,
                student.User.Email);

            // 7. Persist payment record
            _context.Payments.Add(new Payment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                Amount = course.Price,
                OrderId = orderId,
                SnapToken = midtransResult.SnapToken,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new CreatePaymentResult(midtransResult.SnapToken, orderId);
        }

        public async Task HandleNotificationAsync(
            string orderId,
            string transactionStatus,
            string statusCode,
            string grossAmount,
            string receivedSignature)
        {
            // 1. Verify signature
            if (!_midtrans.VerifySignature(orderId, statusCode, grossAmount, receivedSignature))
            {
                _logger.LogWarning(
                    "Invalid signature for order {OrderId}. Possible spoofing attempt.", orderId);
                throw new UnauthorizedAccessException("Invalid signature.");
            }

            // 2. Find payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw new KeyNotFoundException($"Payment not found for order {orderId}.");

            // 3. Update status
            payment.Status = transactionStatus switch
            {
                "settlement" or "capture" => "Success",
                "pending" => "Pending",
                _ => "Failed"
            };

            await _context.SaveChangesAsync();

            // 4. Enroll student on success
            if (payment.Status == "Success")
                await _enrollment.EnrollIfNotAlreadyAsync(payment.StudentId, payment.CourseId, orderId);
        }

        // -------------------------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------------------------

        private async Task ExpireStalePendingPaymentsAsync(int studentId, int courseId)
        {
            var stale = await _context.Payments
                .Where(p =>
                    p.StudentId == studentId &&
                    p.CourseId == courseId &&
                    p.Status == "Pending" &&
                    p.CreatedAt < DateTime.UtcNow.AddHours(-24))
                .ToListAsync();

            if (stale.Count == 0) return;

            foreach (var p in stale)
                p.Status = "Expired";

            await _context.SaveChangesAsync();
        }
    }
}
