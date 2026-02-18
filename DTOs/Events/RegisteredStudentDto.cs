namespace DeWaveFreeAPI.DTOs.Events
{
    public class RegisteredStudentDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentDisplayId { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; }
    }
}
