namespace DeWaveFreeAPI.DTOs.ExternalContent
{
    public class ExternalContentSearchResult
    {
        public string Source { get; set; } = null!;        // "youtube" | "oersi" | "wikimedia"
        public string ExternalId { get; set; } = null!;     // videoId / OERSI id / Commons filename
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Url { get; set; } = null!;           

        // Pre-built so frontend can pass straight into POST /api/content-objects
        public string SuggestedBlockTypeName { get; set; } = null!;  // "Video" | "Image" | "Text"
        public string SuggestedDataJson { get; set; } = null!;
    }
}
