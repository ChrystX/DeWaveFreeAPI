namespace DeWaveFreeAPI.DTOs.ContentObjects
{
    public class ContentObjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int BlockTypeId { get; set; }
        public string BlockTypeName { get; set; } = null!;
        public string? DataJson { get; set; }
        public int Version { get; set; }
        public int? ParentId { get; set; }
        public bool IsDraft { get; set; }
        public List<int>? ChildContentObjectIds { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class CreateContentObjectDto
    {
        public string Title { get; set; } = null!;
        public int BlockTypeId { get; set; }
        public string? DataJson { get; set; }
    }

    public class UpdateContentObjectDto
    {
        public string? Title { get; set; }
        public string? DataJson { get; set; }
    }

    public class PromoteBlockDto
    {
        public string Title { get; set; } = null!;
    }

}
