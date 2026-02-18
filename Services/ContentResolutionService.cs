using DeWaveFreeAPI.Data;

namespace DeWaveFreeAPI.Services
{
    public class ResolvedBlockContent
    {
        public int Id { get; set; }
        public string? DataJson { get; set; }
        public int BlockTypeId { get; set; }
    }
    public class ContentResolutionService
    {
        private readonly DeWaveAPIDbContext _db;

        public ContentResolutionService(DeWaveAPIDbContext db)
        {
            _db = db;

        }
    }
}
