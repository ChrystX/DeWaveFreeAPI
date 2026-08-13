using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Enums.DeWaveFreeAPI.Enums;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{
    public interface IEventRegistrationService
    {
        // Student Registration
        Task<bool> RegisterForEventAsync(int eventId, int studentId);
        Task<bool> CancelRegistrationAsync(int eventId, int studentId);
        Task<bool> CanRegisterAsync(int eventId, int studentId);

        // Get Registrations
        Task<List<RegisteredStudentDto>> GetEventRegistrationsAsync(int eventId);
        Task<bool> IsStudentRegisteredAsync(int eventId, int studentId);

        // Waitlist
        Task<int> GetAvailableSpotsAsync(int eventId);
    }
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly ILogger<EventRegistrationService> _logger;

        public EventRegistrationService(DeWaveAPIDbContext context, ILogger<EventRegistrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> RegisterForEventAsync(int eventId, int studentId)
        {
            // Serializable isolation prevents two concurrent registrations from both
            // reading "not full yet" and overbooking a capacity-limited event (a plain
            // READ COMMITTED transaction does not protect against this phantom read).
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .Include(e => e.CourseEventCourses)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            if (!courseEvent.IsActive)
                throw new InvalidOperationException("Cannot register for inactive event.");

            if (!courseEvent.RequiresRegistration)
                throw new InvalidOperationException("This event does not require registration.");

            if (courseEvent.EventType == EventTypeValues.Seminar)
                throw new InvalidOperationException("Seminars do not require registration.");

            // Check if student can access this event
            if (!await CanStudentAccessEvent(courseEvent, studentId))
                throw new UnauthorizedAccessException("You don't have access to this event.");

            // Check if already registered
            var existingEnrollment = await _context.EventEnrollments
                    .FirstOrDefaultAsync(e => e.EventId == eventId && e.StudentId == studentId);

            if (existingEnrollment != null && existingEnrollment.Status == "registered")
                throw new InvalidOperationException("You are already registered for this event.");


            var registeredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered");

            if (courseEvent.Capacity.HasValue && registeredCount >= courseEvent.Capacity.Value)
                throw new InvalidOperationException("Event is full.");

            if (existingEnrollment != null)
            {
                existingEnrollment.Status = "registered";
                existingEnrollment.RegisteredAt = DateTime.UtcNow;
            }
            else
            {
                _context.EventEnrollments.Add(new EventEnrollment
                {
                    EventId = eventId,
                    StudentId = studentId,
                    RegisteredAt = DateTime.UtcNow,
                    Status = "registered"
                });

                if (courseEvent.TrackAttendance)
                {
                    _context.EventAttendances.Add(new EventAttendance
                    {
                        EventId = eventId,
                        StudentId = studentId,
                        Attended = false,
                        Status = "absent"
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Registration failed, please try again.");
            }

            _logger.LogInformation($"student {studentId} registered for event {eventId}");
            return true;
        }

        public async Task<bool> CancelRegistrationAsync(int eventId, int studentId)
        {
            var enrollment = await _context.EventEnrollments
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.StudentId == studentId);

            if (enrollment == null || enrollment.Status != "registered")
                return false;

            var courseEvent = await _context.CourseEvents.FindAsync(eventId);
            if (courseEvent == null)
                return false;

            // Don't allow cancellation if event has started
            if (courseEvent.StartTime <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel registration after event has started.");

            enrollment.Status = "cancelled";
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Student {studentId} cancelled registration for event {eventId}");
            return true;
        }

        public async Task<bool> CanRegisterAsync(int eventId, int studentId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .Include(e => e.CourseEventCourses)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null || !courseEvent.IsActive)
                return false;

            if (!courseEvent.RequiresRegistration)
                return false;

            if (courseEvent.EventType == EventTypeValues.Seminar)
                return false;

            if (courseEvent.EndTime < DateTime.UtcNow)
                return false;

            // Check if already registered
            var isRegistered = await _context.EventEnrollments
                .AnyAsync(e => e.EventId == eventId && e.StudentId == studentId && e.Status == "registered");

            if (isRegistered)
                return false;

            // Check capacity
            var registeredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered");
            if (courseEvent.Capacity.HasValue && registeredCount >= courseEvent.Capacity.Value)
                return false;

            // Check access
            return await CanStudentAccessEvent(courseEvent, studentId);
        }

        public async Task<List<RegisteredStudentDto>> GetEventRegistrationsAsync(int eventId)
        {
            var registrations = await _context.EventEnrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.EventId == eventId)
                .OrderBy(e => e.RegisteredAt)
                .Select(e => new RegisteredStudentDto
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentDisplayId = e.Student.User.DisplayId,
                    Email = e.Student.User.Email,
                    RegisteredAt = e.RegisteredAt,
                    Status = e.Status
                })
                .ToListAsync();

            return registrations;
        }

        public async Task<bool> IsStudentRegisteredAsync(int eventId, int studentId)
        {
            return await _context.EventEnrollments
                .AnyAsync(e => e.EventId == eventId && e.StudentId == studentId && e.Status == "registered");
        }

        public async Task<int> GetAvailableSpotsAsync(int eventId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null || !courseEvent.Capacity.HasValue)
                return int.MaxValue;

            var registeredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered");
            return Math.Max(0, courseEvent.Capacity.Value - registeredCount);
        }

        private async Task<bool> CanStudentAccessEvent(CourseEvent courseEvent, int studentId)
        {
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

            return false;
        }
    }
}
