using DeWaveFreeAPI.Services.Helpers;
public class CreateEventDto:IEventDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventType { get; set; }     // "online" or "offline"
    public string Visibility { get; set; }    // "course", "public", or "invite"
    public List<int>? CourseIds { get; set; }
    public string? MeetingUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PreviewVideoUrl { get; set; }
    public string? Location { get; set; }
    public bool TrackAttendance { get; set; }
    public bool RequiresRegistration { get; set; }
    public string? Color { get; set; }        // Add for calendar display
    public int? Capacity { get; set; }        // Add for registration limits
}