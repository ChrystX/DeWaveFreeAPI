using DeWaveFreeAPI.DTOs.ExternalContent;
using System.Text.Json;

namespace DeWaveFreeAPI.Services.ExternalContent
{
    public class WikimediaContentService : IExternalContentService
    {
        public string Source => "wikimedia";

        private readonly HttpClient _http;

        public WikimediaContentService(HttpClient http)
        {
            _http = http;
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("DeWaveFreeAPI/1.0 (contact@dewave.example)");
        }

        public async Task<List<ExternalContentSearchResult>> SearchAsync(string query, int limit = 10)
        {
            // Step 1: search for matching file titles
            var searchUrl = "https://commons.wikimedia.org/w/api.php"
                + "?action=query&list=search&srnamespace=6" // namespace 6 = File
                + $"&srlimit={limit}"
                + $"&srsearch={Uri.EscapeDataString(query)}"
                + "&format=json&origin=*";

            var searchResponse = await _http.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode)
                return new List<ExternalContentSearchResult>();

            using var searchStream = await searchResponse.Content.ReadAsStreamAsync();
            using var searchDoc = await JsonDocument.ParseAsync(searchStream);

            var titles = searchDoc.RootElement
                .GetProperty("query")
                .GetProperty("search")
                .EnumerateArray()
                .Select(x => x.GetProperty("title").GetString()!)
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            if (titles.Count == 0)
                return new List<ExternalContentSearchResult>();

            // Step 2: fetch image info (direct URL + thumbnail) for those titles
            var titlesParam = string.Join("|", titles);
            var infoUrl = "https://commons.wikimedia.org/w/api.php"
                + "?action=query&prop=imageinfo"
                + "&iiprop=url|extmetadata"
                + "&iiurlwidth=400"
                + $"&titles={Uri.EscapeDataString(titlesParam)}"
                + "&format=json&origin=*";

            var infoResponse = await _http.GetAsync(infoUrl);
            if (!infoResponse.IsSuccessStatusCode)
                return new List<ExternalContentSearchResult>();

            using var infoStream = await infoResponse.Content.ReadAsStreamAsync();
            using var infoDoc = await JsonDocument.ParseAsync(infoStream);

            var results = new List<ExternalContentSearchResult>();

            var pages = infoDoc.RootElement.GetProperty("query").GetProperty("pages");
            foreach (var page in pages.EnumerateObject())
            {
                var pageVal = page.Value;
                var title = pageVal.GetProperty("title").GetString() ?? "";

                if (!pageVal.TryGetProperty("imageinfo", out var imageInfoArr) || imageInfoArr.GetArrayLength() == 0)
                    continue;

                var imageInfo = imageInfoArr[0];
                var fullUrl = imageInfo.GetProperty("url").GetString() ?? "";
                var thumbUrl = imageInfo.TryGetProperty("thumburl", out var t) ? t.GetString() : fullUrl;

                string? description = null;
                if (imageInfo.TryGetProperty("extmetadata", out var meta)
                    && meta.TryGetProperty("ImageDescription", out var descProp)
                    && descProp.TryGetProperty("value", out var descVal))
                {
                    description = descVal.GetString();
                }

                var displayTitle = title.Replace("File:", "").Trim();

                var dataJson = JsonSerializer.Serialize(new
                {
                    url = fullUrl,
                    alt_text = displayTitle,
                    caption = displayTitle
                });

                results.Add(new ExternalContentSearchResult
                {
                    Source = Source,
                    ExternalId = title,
                    Title = displayTitle,
                    Description = description,
                    ThumbnailUrl = thumbUrl,
                    Url = fullUrl,
                    SuggestedBlockTypeName = "Image",
                    SuggestedDataJson = dataJson
                });
            }

            return results;
        }
    }
}
