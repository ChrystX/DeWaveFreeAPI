namespace DeWaveFreeAPI.Models
{
    public class MidtransSettings
    {
        public string ServerKey { get; set; } = null!;
        public string ClientKey { get; set; } = null!;
        public bool IsProduction { get; set; }
    }
}
