namespace DeWaveFreeAPI.DTOs.CourseInstructor;

public class CourseInstructorCreateDto
{
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public int? SortOrder { get; set; }
}

public class CourseInstructorResponseDto
{
    public int InstructorId { get; set; }
    public string Name { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Certifications { get; set; }
    public string InstructorType { get; set; } = null!;
    public int? SortOrder { get; set; }
}

public class InstructorCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Image { get; set; }
}