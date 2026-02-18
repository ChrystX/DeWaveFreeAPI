namespace DeWaveFreeAPI.DTOs
{
    public class MarkCompleteDto
    {
        public int LessonId { get; set; }
    }

    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int Percentage { get; set; }
    }
}
