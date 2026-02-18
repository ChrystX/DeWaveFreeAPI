using System.ComponentModel.DataAnnotations;

namespace DeWaveFreeAPI.DTOs.Events
{
    public class BulkMarkAttendanceDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public List<StudentAttendanceInput> Attendances { get; set; } = new List<StudentAttendanceInput>();
    }

    public class StudentAttendanceInput
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public bool Attended { get; set; }

        public DateTime? JoinedAt { get; set; }
    }
}