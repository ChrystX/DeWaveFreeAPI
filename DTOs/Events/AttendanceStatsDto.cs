namespace DeWaveFreeAPI.DTOs.Events
{
    public class AttendanceStatsDto
    {
        public int TotalRegistered { get; set; }
        public int TotalAttended { get; set; }
        public double AttendanceRate { get; set; }
    }
}
