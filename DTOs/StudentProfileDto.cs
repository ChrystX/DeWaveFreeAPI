using System.ComponentModel.DataAnnotations;

namespace DeWaveFreeAPI.DTOs
{
    public class StudentProfileDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = null!;

        [StringLength(14)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        [StringLength(255)]
        public string? EmergencyContact { get; set; }

        [StringLength(14)]
        public string? EmergencyPhone { get; set; }
    }
}
