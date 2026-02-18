namespace DeWaveFreeAPI.DTOs.Events
{
    public class EventCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;  // Will contain Course.Title
        public string? CourseCode { get; set; }  // Will always be null since Course doesn't have it
    }
}
