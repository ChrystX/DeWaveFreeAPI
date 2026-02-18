using DeWaveFreeAPI.DTOs.ContentObjects;

namespace DeWaveFreeAPI.DTOs.Blocks
{
    public class LessonBlockDto
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public int BlockTypeId { get; set; }

        public string BlockTypeName { get; set; } = "";

        public int OrderIndex { get; set; }

        public string DataJson { get; set; } = "{}";

        public bool IsComposite { get; set; }

        public List<ContentObjectDto>? Children { get; set; }

        public int? ContentObjectId { get; set; }

        public int? ForkedFromContentObjectId { get; set; }
    }

    public class UpdateBlockDto
    {
        public int OrderIndex { get; set; }

        public string DataJson { get; set; } = "{}";

        public int? ContentObjectId { get; set; }
    }

    public class CreateBlockDto
    {
        public int BlockTypeId { get; set; }

        public int OrderIndex { get; set; }

        public string DataJson { get; set; } = "{}";

        public int? ContentObjectId { get; set; }
    }
}
