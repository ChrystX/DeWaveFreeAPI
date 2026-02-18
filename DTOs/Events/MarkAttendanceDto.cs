namespace DeWaveFreeAPI.DTOs.Events
{
    // For instructor marking attendance
    public class MarkAttendanceDto
    {
        public int EventId { get; set; }
        public int StudentId { get; set; }
        public bool Attended { get; set; }
    }

    // For student self-check-in (StudentId from auth context)
    public class CheckInDto
    {
        public int EventId { get; set; }
        // No StudentId - get from ClaimsPrincipal
    }
}
