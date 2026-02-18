using System.Text.Json;
using DeWaveFreeAPI.DTOs.ExternalContent;

namespace DeWaveFreeAPI.Services.ExternalContent
{
    public class YouTubeContentService : IExternalContentService
    {
        public string Source => "youtube";

        private readonly HttpClient _http;
        private readonly string _apiKey;

        public YouTubeContentService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ExternalContent:YouTubeApiKey"]
                ?? throw new InvalidOperationException("YouTube API key not configured");
        }

        public async Task<List<ExternalContentSearchResult>> SearchAsync(string query, int limit = 10)
        {
            var url = "https://www.googleapis.com/youtube/v3/search"
                + $"?part=snippet&type=video&maxResults={limit}"
                + $"&q={Uri.EscapeDataString(query)}"
                + $"&key={_apiKey}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<ExternalContentSearchResult>();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var results = new List<ExternalContentSearchResult>();

            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                var videoId = item.GetProperty("id").GetProperty("videoId").GetString();
                var snippet = item.GetProperty("snippet");

                var title = snippet.GetProperty("title").GetString() ?? "";
                var description = snippet.TryGetProperty("description", out var d) ? d.GetString() : null;
                var thumbnail = snippet.GetProperty("thumbnails").GetProperty("medium").GetProperty("url").GetString();

                var videoUrl = $"https://www.youtube.com/watch?v={videoId}";

                var dataJson = JsonSerializer.Serialize(new
                {
                    url = videoUrl,
                    caption = title
                });

                results.Add(new ExternalContentSearchResult
                {
                    Source = Source,
                    ExternalId = videoId ?? "",
                    Title = title,
                    Description = description,
                    ThumbnailUrl = thumbnail,
                    Url = videoUrl,
                    SuggestedBlockTypeName = "Video",
                    SuggestedDataJson = dataJson
                });
            }

            return results;
        }
    }
}
