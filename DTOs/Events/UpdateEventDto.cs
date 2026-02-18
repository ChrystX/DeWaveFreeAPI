using System.ComponentModel.DataAnnotations;
using DeWaveFreeAPI.Services.Helpers;

namespace DeWaveFreeAPI.DTOs.Events
{
    public class UpdateEventDto : IEventDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(20)]
        public string EventType { get; set; }

        [Required]
        [StringLength(20)]
        public string Visibility { get; set; }

        public string? ThumbnailUrl { get; set; }
        public string? PreviewVideoUrl { get; set; }

        public List<int>? CourseIds { get; set; }

        [StringLength(255)]
        public string? MeetingUrl { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        public bool TrackAttendance { get; set; }

        public bool RequiresRegistration { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int? Capacity { get; set; }

        public bool IsActive { get; set; }
    }
}
