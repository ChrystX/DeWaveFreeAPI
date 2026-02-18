namespace DeWaveFreeAPI.DTOs.ContentObjects
{
    public class GroupContentObjectsDto
    {
        public string Title { get; set; } = null!;
        public List<int> ChildContentObjectIds { get; set; } = new();
    }

    public class GroupBlocksDto
    {
        public string Title { get; set; } = null!;
        public List<int> BlockIds { get; set; } = new();
    }
}
