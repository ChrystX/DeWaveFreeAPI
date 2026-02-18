using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Enums.DeWaveFreeAPI.Enums;
using DeWaveFreeAPI.Models;
using System.Linq;

namespace DeWaveFreeAPI.Services.Helpers
{
    public interface IEventDto
    {
        string Title { get; }
        string? Description { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        string EventType { get; }
        string Visibility { get; }
        string? MeetingUrl { get; }
        string? Location { get; }
        bool RequiresRegistration { get; }
        List<int>? CourseIds { get; }
    }
    public static class EventHelpers
    {
        public static void ValidateEventDto(IEventDto dto)
        {
            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("EndTime must be after StartTime.");

            if (dto.EventType.ToLower() == EventTypeValues.Online && string.IsNullOrEmpty(dto.MeetingUrl))
                throw new ArgumentException("MeetingUrl is required for online events.");

            if (dto.EventType.ToLower() == EventTypeValues.Offline && string.IsNullOrEmpty(dto.Location))
                throw new ArgumentException("Location is required for offline events.");

            if (dto.Visibility.ToLower() == VisibilityValues.Course &&
                (dto.CourseIds == null || !dto.CourseIds.Any()))
                throw new ArgumentException("At least one CourseId is required for course-visibility events.");

            if (dto.EventType.ToLower() == EventTypeValues.Seminar && dto.RequiresRegistration)
                throw new ArgumentException("Seminars cannot require registration.");
        }

        public static IQueryable<CourseEvent> ApplyFilters(IQueryable<CourseEvent> query, EventFilterDto filter)
        {
            if (filter.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(e => e.EndTime <= filter.EndDate.Value);

            if (filter.EventTypes != null && filter.EventTypes.Any())
                query = query.Where(e => filter.EventTypes.Contains(e.EventType));

            if (filter.Visibilities != null && filter.Visibilities.Any())
                query = query.Where(e => filter.Visibilities.Contains(e.Visibility));

            if (filter.OnlyUpcoming.HasValue && filter.OnlyUpcoming.Value)
                query = query.Where(e => e.StartTime > DateTime.UtcNow);

            return query;
        }

        public static string CalculateEventStatus(CourseEvent e, bool isRegistered, bool isAttended, DateTime now)
        {
            if (e.EventType == EventTypeValues.Seminar)
                return "info";

            if (e.StartTime > now)
                return "upcoming";

            if (e.EndTime < now)
            {
                if (isAttended) return "attended";
                if (isRegistered) return "missed";
                return "past";    // ← was "info", now actionable
            }

            // Currently live
            if (isAttended) return "attended";
            if (isRegistered) return "live";   // ← was "upcoming", distinguish live from upcoming
            return "live-unregistered";        // ← was "info"
        }

        public static bool CalculateCanRegister(CourseEvent e, bool isRegistered, int registeredCount)
        {
            if (isRegistered) return false;
            if (!e.RequiresRegistration) return false;
            if (e.EventType == EventTypeValues.Seminar) return false;
            if (e.EndTime < DateTime.UtcNow) return false;
            if (e.Capacity.HasValue && registeredCount >= e.Capacity.Value) return false;

            return true;
        }
    }
}