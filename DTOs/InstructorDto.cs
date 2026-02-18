namespace DeWaveFreeAPI.DTOs
{
    public class InstructorDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Certifications { get; set; }
        public string? Headline { get; set; }
        public string? Specialization { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class InstructorCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Certifications { get; set; }
        public string? Headline { get; set; }
        public string? Specialization { get; set; }
    }

}
