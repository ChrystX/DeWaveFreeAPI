namespace DeWaveFreeAPI.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string token);
        Task SendPasswordResetEmailAsync(string email, string token);
    }
    public class EmailService : IEmailService
    {
        private readonly ILogger<IEmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<IEmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            _logger.LogInformation($"Verification email would be sent to {email} with token {token}");
            await Task.CompletedTask;
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            _logger.LogInformation($"Password reset email would be sent to {email} with token {token}");
            await Task.CompletedTask;
        }
    }
}