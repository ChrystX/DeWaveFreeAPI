namespace DeWaveFreeAPI.DTOs.Events
{
    public class EventDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; }  // Changed from byte to string
        public string Visibility { get; set; }
        public string? MeetingUrl { get; set; }
        public string? Location { get; set; }
        public bool TrackAttendance { get; set; }
        public bool RequiresRegistration { get; set; }
        public string? Color { get; set; }
        public int? Capacity { get; set; }
        public bool IsActive { get; set; }  // Added
        public DateTime CreatedAt { get; set; }  // Added

        // Creator info
        public int CreatedByUserId { get; set; }
        public string CreatedByName { get; set; }
        public string CreatorRole { get; set; }

        // Course info
        public List<int> CourseIds { get; set; }  // Just IDs
        public List<EventCourseDto> Courses { get; set; } = new List<EventCourseDto>();  // Added - Full course details

        // Statistics
        public int RegisteredCount { get; set; }  // Added - Changed from nullable
        public int AttendedCount { get; set; }  // Added - Changed from nullable
    }
}