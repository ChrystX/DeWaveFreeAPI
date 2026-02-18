namespace DeWaveFreeAPI.DTOs.Events
{
    public class StudentEventResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; }
        public string? MeetingUrl { get; set; }  // Only if registered
        public string? Location { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewVideoUrl { get; set; }
        public string Status { get; set; }  // upcoming/missed/attended/info
        public bool IsRegistered { get; set; }
        public bool IsAttended { get; set; }
        public bool TrackAttendance { get; set; }
        public bool RequiresRegistration { get; set; }
        public bool CanRegister { get; set; }  // Based on capacity & enrollment

        public string? Description { get; set; }
    }

    public class InstructorEventResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; }
        public string Visibility { get; set; }
        public string MeetingUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewVideoUrl { get; set; }
        public string Location { get; set; }           
        public string Color { get; set; }             
        public int? Capacity { get; set; }
        public int RegisteredCount { get; set; }
        public int AttendedCount { get; set; }
        public bool IsActive { get; set; }
        public bool TrackAttendance { get; set; }     
        public bool RequiresRegistration { get; set; }
        public List<int> CourseIds { get; set; }
    }
}
