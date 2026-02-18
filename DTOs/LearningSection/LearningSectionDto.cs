namespace DeWaveFreeAPI.DTOs.LearningSections
{
    public class LearningSectionDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string Title { get; set; } = null!;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

    }
    public class CreateLearningSectionDto
    {
        public string Title { get; set; } = null!;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class UpdateLearningSectionDto
    {
        public string Title { get; set; } = null!;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }

    public class SyllabusSectionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int SortOrder { get; set; }
        public List<SyllabusLessonDto> Lessons { get; set; } = new();
    }

    public class SyllabusLessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int SortOrder { get; set; }
    }

}
