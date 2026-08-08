using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Enums.DeWaveFreeAPI.Enums;
using DeWaveFreeAPI.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{
    public interface IEventQueryService
    {
        Task<EventDetailDto> GetEventDetailAsync(int eventId);
        Task<List<EventListDto>> GetEventsAsync(EventFilterDto filter, int? userId = null, string? role = null);
        Task<List<EventListDto>> GetStudentEventsAsync(int studentId, EventFilterDto filter);
        Task<List<EventListDto>> GetInstructorEventsAsync(int userId, EventFilterDto filter);
        Task<StudentEventResponseDto> GetStudentEventDetailAsync(int eventId, int studentId);
        Task<InstructorEventResponseDto> GetInstructorEventDetailAsync(int eventId, int userId);
    }

    public class EventQueryService : IEventQueryService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly IEventAccessService _accessService;

        public EventQueryService(DeWaveAPIDbContext context, IEventAccessService accessService)
        {
            _context = context;
            _accessService = accessService;
        }

        public async Task<EventDetailDto> GetEventDetailAsync(int eventId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.CreatedByUser).ThenInclude(u => u.Role)
                .Include(e => e.CreatedByUser.Instructor)
                .Include(e => e.CourseEventCourses).ThenInclude(cec => cec.Course)
                .Include(e => e.EventEnrollments)
                .Include(e => e.EventAttendances)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            var creatorName = courseEvent.CreatedByUser.Instructor?.Name
                ?? courseEvent.CreatedByUser.Username;

            return new EventDetailDto
            {
                Id = courseEvent.Id,
                Title = courseEvent.Title,
                Description = courseEvent.Description,
                StartTime = courseEvent.StartTime,
                EndTime = courseEvent.EndTime,
                EventType = courseEvent.EventType,
                Visibility = courseEvent.Visibility,
                MeetingUrl = courseEvent.MeetingUrl,
                Location = courseEvent.Location,
                TrackAttendance = courseEvent.TrackAttendance,
                RequiresRegistration = courseEvent.RequiresRegistration,
                Color = courseEvent.Color,
                Capacity = courseEvent.Capacity,
                IsActive = courseEvent.IsActive,
                CreatedAt = courseEvent.CreatedAt,
                CreatedByUserId = courseEvent.CreatedByUserId,
                CreatedByName = creatorName,
                CreatorRole = courseEvent.CreatedByUser.Role.Name,
                CourseIds = courseEvent.CourseEventCourses.Select(cec => cec.CourseId).ToList(),
                Courses = courseEvent.CourseEventCourses.Select(cec => new EventCourseDto
                {
                    CourseId = cec.CourseId,
                    CourseName = cec.Course.Title,
                    CourseCode = null
                }).ToList(),
                RegisteredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered"),
                AttendedCount = courseEvent.EventAttendances.Count(a => a.Attended)
            };
        }

        public async Task<List<EventListDto>> GetEventsAsync(EventFilterDto filter, int? userId = null, string? role = null)
        {
            if (role == "student" && userId.HasValue)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId.Value);
                if (student != null)
                    return await GetStudentEventsAsync(student.Id, filter);
            }
            else if ((role == "instructor" || role == "admin") && userId.HasValue)
            {
                return await GetInstructorEventsAsync(userId.Value, filter);
            }

            // Unauthenticated / unrecognised role: public events only
            var query = _context.CourseEvents
                .Where(e => e.IsActive && e.Visibility == VisibilityValues.Public);

            query = EventHelpers.ApplyFilters(query, filter);

            var events = await query.OrderBy(e => e.StartTime).ToListAsync();

            return events.Select(e => new EventListDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                EventType = e.EventType,
                Visibility = e.Visibility,
                Color = e.Color,
                RequiresRegistration = e.RequiresRegistration,
                Status = e.StartTime > DateTime.UtcNow ? "upcoming" : "past"
            }).ToList();
        }

        public async Task<List<EventListDto>> GetStudentEventsAsync(int studentId, EventFilterDto filter)
        {
            var studentCourseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId && sc.IsActive)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            var query = _context.CourseEvents
                .Include(e => e.CourseEventCourses)
                .Include(e => e.EventEnrollments)
                .Include(e => e.EventAttendances)
                .Where(e => e.IsActive)
                .Where(e =>
                    e.Visibility == VisibilityValues.Public ||
                    (e.Visibility == VisibilityValues.Course &&
                     e.CourseEventCourses.Any(cec => studentCourseIds.Contains(cec.CourseId))));

            query = EventHelpers.ApplyFilters(query, filter);

            var events = await query.OrderBy(e => e.StartTime).ToListAsync();
            var now = DateTime.UtcNow;

            return events.Select(e =>
            {
                var isRegistered = e.EventEnrollments.Any(en => en.StudentId == studentId && en.Status == "registered");
                var isAttended = e.EventAttendances.Any(a => a.StudentId == studentId && a.Attended);

                return new EventListDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    EventType = e.EventType,
                    Visibility = e.Visibility,
                    Color = e.Color,
                    RequiresRegistration = e.RequiresRegistration,
                    Status = EventHelpers.CalculateEventStatus(e, isRegistered, isAttended, now)
                };
            }).ToList();
        }

        public async Task<List<EventListDto>> GetInstructorEventsAsync(int userId, EventFilterDto filter)
        {
            var query = _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .Include(e => e.EventAttendances)
                .Where(e => e.CreatedByUserId == userId && e.IsActive);

            query = EventHelpers.ApplyFilters(query, filter);

            var events = await query.OrderBy(e => e.StartTime).ToListAsync();

            return events.Select(e => new EventListDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                EventType = e.EventType,
                Visibility = e.Visibility,
                Color = e.Color,
                RequiresRegistration = e.RequiresRegistration,
                Status = e.StartTime > DateTime.UtcNow ? "upcoming" : "past",
                RegisteredCount = e.EventEnrollments.Count(en => en.Status == "registered"),
                AttendedCount = e.EventAttendances.Count(a => a.Attended)
            }).ToList();
        }

        public async Task<StudentEventResponseDto> GetStudentEventDetailAsync(int eventId, int studentId)
        {
            if (!await _accessService.CanStudentAccessEventAsync(eventId, studentId))
                throw new UnauthorizedAccessException("You don't have access to this event.");

            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .Include(e => e.EventAttendances)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            var isRegistered = courseEvent.EventEnrollments
                .Any(en => en.StudentId == studentId && en.Status == "registered");
            var isAttended = courseEvent.EventAttendances
                .Any(a => a.StudentId == studentId && a.Attended);
            var registeredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered");

            return new StudentEventResponseDto
            {
                Id = courseEvent.Id,
                Title = courseEvent.Title,
                StartTime = courseEvent.StartTime,
                EndTime = courseEvent.EndTime,
                EventType = courseEvent.EventType,
                ThumbnailUrl = courseEvent.ThumbnailUrl,
                PreviewVideoUrl = courseEvent.PreviewVideoUrl,
                MeetingUrl = (isRegistered || !courseEvent.RequiresRegistration)
                    ? courseEvent.MeetingUrl
                    : null,
                Location = courseEvent.Location,
                Status = EventHelpers.CalculateEventStatus(courseEvent, isRegistered, isAttended, DateTime.UtcNow),
                IsRegistered = isRegistered,
                Description = courseEvent.Description,
                IsAttended = isAttended,
                TrackAttendance = courseEvent.TrackAttendance,
                RequiresRegistration = courseEvent.RequiresRegistration,
                CanRegister = EventHelpers.CalculateCanRegister(courseEvent, isRegistered, registeredCount)
            };
        }

        public async Task<InstructorEventResponseDto> GetInstructorEventDetailAsync(int eventId, int userId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                .Include(e => e.EventAttendances)
                .Include(e => e.CourseEventCourses)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            await _accessService.EnsureEventOwnerOrAdminAsync(courseEvent, userId);

            return new InstructorEventResponseDto
            {
                Id = courseEvent.Id,
                Title = courseEvent.Title,
                Description = courseEvent.Description,
                StartTime = courseEvent.StartTime,
                EndTime = courseEvent.EndTime,
                EventType = courseEvent.EventType,
                ThumbnailUrl = courseEvent.ThumbnailUrl,
                PreviewVideoUrl = courseEvent.PreviewVideoUrl,
                Visibility = courseEvent.Visibility,
                MeetingUrl = courseEvent.MeetingUrl,
                Location = courseEvent.Location,
                Color = courseEvent.Color,
                Capacity = courseEvent.Capacity,
                IsActive = courseEvent.IsActive,
                TrackAttendance = courseEvent.TrackAttendance,
                RequiresRegistration = courseEvent.RequiresRegistration,
                CourseIds = courseEvent.CourseEventCourses.Select(cec => cec.CourseId).ToList(),
                RegisteredCount = courseEvent.EventEnrollments.Count(e => e.Status == "registered"),
                AttendedCount = courseEvent.EventAttendances.Count(a => a.Attended),
            };
        }
    }
}