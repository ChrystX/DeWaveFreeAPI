using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Enums.DeWaveFreeAPI.Enums;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Services
{
    public interface IEventCrudService
    {
        Task<int> CreateEventAsync(CreateEventDto dto, int userId);
        Task<bool> UpdateEventAsync(int eventId, UpdateEventDto dto, int userId);
        Task<bool> DeleteEventAsync(int eventId, int userId);
        Task<bool> ToggleEventStatusAsync(int eventId, int userId, bool isActive);
    }

    public class EventCrudService : IEventCrudService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly IEventAccessService _accessService;
        private readonly ILogger<EventCrudService> _logger;

        public EventCrudService(
            DeWaveAPIDbContext context,
            IEventAccessService accessService,
            ILogger<EventCrudService> logger)
        {
            _context = context;
            _accessService = accessService;
            _logger = logger;
        }

        public async Task<int> CreateEventAsync(CreateEventDto dto, int userId)
        {
            if (!EventTypeValues.IsValid(dto.EventType))
                throw new ArgumentException($"Invalid EventType. Must be one of: {string.Join(", ", EventTypeValues.All)}");

            if (!VisibilityValues.IsValid(dto.Visibility))
                throw new ArgumentException($"Invalid Visibility. Must be one of: {string.Join(", ", VisibilityValues.All)}");

            EventHelpers.ValidateEventDto(dto);

            await _accessService.EnsureRoleAsync(userId, "admin", "instructor");

            var courseEvent = new CourseEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                EventType = dto.EventType.ToLower(),
                Visibility = dto.Visibility.ToLower(),
                MeetingUrl = dto.MeetingUrl,
                Location = dto.Location,
                TrackAttendance = dto.TrackAttendance,
                RequiresRegistration = dto.RequiresRegistration,
                Color = dto.Color,
                Capacity = dto.Capacity,
                ThumbnailUrl = dto.ThumbnailUrl,
                PreviewVideoUrl = dto.PreviewVideoUrl,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.CourseEvents.Add(courseEvent);
            await _context.SaveChangesAsync();

            if (dto.Visibility.ToLower() == VisibilityValues.Course && dto.CourseIds?.Any() == true)
                await LinkCoursesToEventAsync(courseEvent.Id, dto.CourseIds);

            _logger.LogInformation("Event {EventId} created by user {UserId}", courseEvent.Id, userId);
            return courseEvent.Id;
        }

        public async Task<bool> UpdateEventAsync(int eventId, UpdateEventDto dto, int userId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.CourseEventCourses)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                return false;

            await _accessService.EnsureEventOwnerOrAdminAsync(courseEvent, userId);

            EventHelpers.ValidateEventDto(dto);

            courseEvent.Title = dto.Title;
            courseEvent.Description = dto.Description;
            courseEvent.StartTime = dto.StartTime;
            courseEvent.EndTime = dto.EndTime;
            courseEvent.EventType = dto.EventType.ToLower();
            courseEvent.Visibility = dto.Visibility.ToLower();
            courseEvent.MeetingUrl = dto.MeetingUrl;
            courseEvent.Location = dto.Location;
            courseEvent.TrackAttendance = dto.TrackAttendance;
            courseEvent.RequiresRegistration = dto.RequiresRegistration;
            courseEvent.Color = dto.Color;
            courseEvent.Capacity = dto.Capacity;
            courseEvent.ThumbnailUrl = dto.ThumbnailUrl;
            courseEvent.PreviewVideoUrl = dto.PreviewVideoUrl;
            courseEvent.IsActive = dto.IsActive;

            if (dto.Visibility.ToLower() == VisibilityValues.Course && dto.CourseIds != null)
            {
                _context.CourseEventCourses.RemoveRange(courseEvent.CourseEventCourses);
                await LinkCoursesToEventAsync(eventId, dto.CourseIds);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Event {EventId} updated by user {UserId}", eventId, userId);
            return true;
        }

        public async Task<bool> DeleteEventAsync(int eventId, int userId)
        {
            var courseEvent = await _context.CourseEvents
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                return false;

            await _accessService.EnsureEventOwnerOrAdminAsync(courseEvent, userId);

            // Soft delete
            courseEvent.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event {EventId} soft-deleted by user {UserId}", eventId, userId);
            return true;
        }

        public async Task<bool> ToggleEventStatusAsync(int eventId, int userId, bool isActive)
        {
            var courseEvent = await _context.CourseEvents
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                return false;

            await _accessService.EnsureEventOwnerOrAdminAsync(courseEvent, userId);

            courseEvent.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task LinkCoursesToEventAsync(int eventId, List<int> courseIds)
        {
            var links = courseIds.Select(courseId => new CourseEventCourse
            {
                EventId = eventId,
                CourseId = courseId,
                CreatedAt = DateTime.UtcNow
            });

            _context.CourseEventCourses.AddRange(links);
            await _context.SaveChangesAsync();
        }
    }
}