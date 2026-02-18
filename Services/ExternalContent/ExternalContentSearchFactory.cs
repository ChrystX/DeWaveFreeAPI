namespace DeWaveFreeAPI.Services.ExternalContent
{
    public class ExternalContentSearchFactory
    {
        private readonly Dictionary<string, IExternalContentService> _services;

        public ExternalContentSearchFactory(IEnumerable<IExternalContentService> services)
        {
            _services = services.ToDictionary(s => s.Source, StringComparer.OrdinalIgnoreCase);
        }

        public IExternalContentService? Get(string source) =>
            _services.TryGetValue(source, out var svc) ? svc : null;

        public IEnumerable<string> AvailableSources => _services.Keys;
    }
}
