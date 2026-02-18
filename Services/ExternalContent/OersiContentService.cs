using System.Text;
using System.Text.Json;
using DeWaveFreeAPI.DTOs.ExternalContent;

namespace DeWaveFreeAPI.Services.ExternalContent
{
    public class OersiContentService : IExternalContentService
    {
        public string Source => "oersi";

        private readonly HttpClient _http;

        public OersiContentService(HttpClient http)
        {
            _http = http;
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("DeWaveFreeAPI/1.0 (contact@dewave.example)");
        }
        public async Task<List<ExternalContentSearchResult>> SearchAsync(string query, int limit = 10)
        {
            var url = "https://oersi.org/api/search/oer_data/_search";

            var body = new
            {
                size = limit,
                query = new
                {
                    multi_match = new
                    {
                        query,
                        fields = new[] { "name", "description", "keywords" }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<ExternalContentSearchResult>();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var results = new List<ExternalContentSearchResult>();

            if (!doc.RootElement.TryGetProperty("hits", out var hitsWrapper)
                || !hitsWrapper.TryGetProperty("hits", out var hits))
                return results;

            foreach (var hit in hits.EnumerateArray())
            {
                var source = hit.GetProperty("_source");

                var title = source.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var description = source.TryGetProperty("description", out var desc) ? desc.GetString() : null;

                // "id" field is typically the resource URL itself in OERSI
                var resourceUrl = source.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";

                // thumbnails are sometimes under "image"
                string? thumbnail = source.TryGetProperty("image", out var img) ? img.GetString() : null;

                var creator = source.TryGetProperty("creator", out var creatorArr) && creatorArr.GetArrayLength() > 0
                    ? creatorArr[0].TryGetProperty("name", out var cn) ? cn.GetString() : null
                    : null;

                var license = source.TryGetProperty("license", out var lic) ? lic.GetString() : null;

                var dataJson = JsonSerializer.Serialize(new
                {
                    title,
                    description,
                    url = resourceUrl,
                    creator,
                    license
                });

                results.Add(new ExternalContentSearchResult
                {
                    Source = Source,
                    ExternalId = resourceUrl,
                    Title = title,
                    Description = description,
                    ThumbnailUrl = thumbnail,
                    Url = resourceUrl,
                    SuggestedBlockTypeName = "ExternalLink",
                    SuggestedDataJson = dataJson
                });
            }

            return results;
        }
    }
}
