using DeWaveFreeAPI.DTOs.ExternalContent;

namespace DeWaveFreeAPI.Services.ExternalContent
{
    public interface IExternalContentService
    {
        string Source { get; }
        Task<List<ExternalContentSearchResult>> SearchAsync(string query, int limit = 10);
    }
}
