using System.Net;
using System.Net.Mail;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
    Task SendAccountSetupEmailAsync(string email, string token);
}

public class EmailService : IEmailService
{
    private readonly ILogger<IEmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public EmailService(ILogger<IEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _baseUrl = _configuration["App:BaseUrl"]; // make sure this is set in appsettings
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        var link = $"{_baseUrl}/verify-email?token={token}";
        var subject = "Verify your email";
        var body = $"""
            Hello,

            Please verify your email address by clicking the link below:

            {link}

            This link expires in 24 hours.
            """;
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var link = $"{_baseUrl}/reset-password?token={token}";
        var subject = "Reset your password";
        var body = $"""
            Hello,

            Click the link below to reset your password:

            {link}

            This link expires in 1 hour. If you did not request this, you can ignore it.
            """;
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendAccountSetupEmailAsync(string email, string token)
    {
        var link = $"{_baseUrl}/reset-password?token={token}";
        var subject = "Welcome — Set up your account password";
        var body = $"""
            Hello,

            An account has been created for you. Click the link below to set your password and get started:

            {link}

            This link expires in 1 hour.
            """;
        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer = _configuration["Email:SmtpServer"];
        var port = int.Parse(_configuration["Email:Port"]);
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromAddress = _configuration["Email:FromAddress"];

        var mail = new MailMessage();
        mail.From = new MailAddress(fromAddress);
        mail.To.Add(toEmail);
        mail.Subject = subject;
        mail.Body = body;

        var smtp = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        await smtp.SendMailAsync(mail);
        _logger.LogInformation($"Email sent to {toEmail}");
    }
}                                     