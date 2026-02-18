using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Enums.DeWaveFreeAPI.Enums;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{
    public interface IEventAccessService
    {
        Task<bool> CanStudentAccessEventAsync(int eventId, int studentId);
        Task EnsureRoleAsync(int userId, params string[] allowedRoles);
        Task EnsureEventOwnerOrAdminAsync(CourseEvent courseEvent, int userId);
    }

    public class EventAccessService : IEventAccessService
    {
        private readonly DeWaveAPIDbContext _context;

        public EventAccessService(DeWaveAPIDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanStudentAccessEventAsync(int eventId, int studentId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.CourseEventCourses)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null || !courseEvent.IsActive)
                return false;

            if (courseEvent.Visibility == VisibilityValues.Public)
                return true;

            if (courseEvent.Visibility == VisibilityValues.Course)
            {
                var courseIds = courseEvent.CourseEventCourses.Select(cec => cec.CourseId).ToList();
                return await _context.StudentCourses
                    .AnyAsync(sc => sc.StudentId == studentId &&
                                   courseIds.Contains(sc.CourseId) &&
                                   sc.IsActive);
            }

            if (courseEvent.Visibility == VisibilityValues.Invite)
            {
                return await _context.EventEnrollments
                    .AnyAsync(ee => ee.EventId == eventId && ee.StudentId == studentId);
            }

            return false;
        }

        /// <summary>
        /// Fetches the user and throws if not found or not in one of the allowed roles.
        /// Pass no roles to skip role validation.
        /// </summary>
        public async Task EnsureRoleAsync(int userId, params string[] allowedRoles)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (allowedRoles.Length > 0 && !allowedRoles.Contains(user.Role.Name, StringComparer.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    $"Only {string.Join(" and ", allowedRoles)}s can perform this action.");
        }

        /// <summary>
        /// Throws if the user is neither the event creator nor an Admin.
        /// </summary>
        public async Task EnsureEventOwnerOrAdminAsync(CourseEvent courseEvent, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (courseEvent.CreatedByUserId != userId &&
                !string.Equals(user.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You don't have permission to modify this event.");
        }

    }
}