using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{
    public interface IEnrollmentService
    {
        Task EnrollIfNotAlreadyAsync(int studentId, int courseId, string orderId);
    }
    public class EnrollmentService : IEnrollmentService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(
            DeWaveAPIDbContext context,
            ILogger<EnrollmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EnrollIfNotAlreadyAsync(int studentId, int courseId, string orderId)
        {
            var alreadyEnrolled = await _context.StudentCourses.AnyAsync(e =>
                e.StudentId == studentId &&
                e.CourseId == courseId);

            if (alreadyEnrolled)
                return;

            _context.StudentCourses.Add(new StudentCourse
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true

            });

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Student {StudentId} enrolled in course {CourseId} via order {OrderId}.",
                studentId, courseId, orderId);
        }
    }
}
