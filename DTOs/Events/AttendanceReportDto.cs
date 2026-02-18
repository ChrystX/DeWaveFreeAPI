namespace DeWaveFreeAPI.DTOs.Events
{
    public class AttendanceReportDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalRegistered { get; set; }
        public int TotalAttended { get; set; }
        public int TotalAbsent { get; set; }
        public double AttendanceRate { get; set; }

        public List<StudentAttendanceReportDto> Students { get; set; } = new List<StudentAttendanceReportDto>();
    }

    public class StudentAttendanceReportDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentDisplayId { get; set; }
        public bool IsRegistered { get; set; }
        public bool Attended { get; set; }
        public DateTime? JoinedAt { get; set; }
        public string Status { get; set; }
    }
}
