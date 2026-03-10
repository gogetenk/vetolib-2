using System.Net;
using System.Net.Mail;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure.Email;

public class SmtpEmailOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = false;
    public string FromAddress { get; set; } = "noreply@vetolib.ae";
    public string FromName { get; set; } = "Vetolib";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = _options.Username is not null
                    ? new NetworkCredential(_options.Username, _options.Password)
                    : null
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            if (message.PlainTextBody is not null)
            {
                mail.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        message.PlainTextBody, null, "text/plain"));
            }

            mail.To.Add(message.To);

            await client.SendMailAsync(mail, ct);

            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", message.To, message.Subject);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", message.To);
            return Result.Error($"Failed to send email: {ex.Message}");
        }
    }
}
