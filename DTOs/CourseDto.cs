using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DeWaveFreeAPI.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public int? Duration { get; set; }
        public int? VideoCount { get; set; }
        public decimal? Rating { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? Price { get; set; }
        public int InstructorId { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
