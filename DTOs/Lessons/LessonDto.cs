namespace DeWaveFreeAPI.DTOs.Lessons
{
    public class LessonDto
    {
        public int Id { get; set; }

        public int SectionId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public string LessonType { get; set; } = "lesson";

        public string? SettingsJson { get; set; }
    }

    public class CreateLessonDto
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public string LessonType { get; set; } = "lesson";
    }

    public class UpdateLessonDto
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class LessonWithBlocksDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public List<LessonBlockDto> Blocks { get; set; }
    }

    public class LessonBlockDto
    {
        public int Id { get; set; }
        public int BlockTypeId { get; set; }
        public int OrderIndex { get; set; }
        public string? DataJson { get; set; }
    }
}
