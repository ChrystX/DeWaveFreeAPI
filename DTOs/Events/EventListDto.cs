public class EventListDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventType { get; set; }
    public string Visibility { get; set; }
    public string? Color { get; set; }
    public bool RequiresRegistration { get; set; }
    public string Status { get; set; }
    public int RegisteredCount { get; set; }
    public int AttendedCount { get; set; }
}