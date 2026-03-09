using Ardalis.Result;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// MailKit-based SMTP email sender.
/// Replaces the System.Net.Mail implementation for better
/// async support and compatibility with MailHog / production SMTP.
/// </summary>
internal class MailKitEmailSender : IEmailSender
{
    private readonly MailKitSmtpOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<MailKitSmtpOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.PlainTextBody
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            var socketOptions = _options.EnableSsl
                ? MailKit.Security.SecureSocketOptions.StartTls
                : MailKit.Security.SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, ct);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);

            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(quit: true, ct);

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

internal class MailKitSmtpOptions
{
    public const string SectionName = "Notifications:Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = false;
    public string FromAddress { get; set; } = "noreply@desertpaws.ae";
    public string FromName { get; set; } = "Desert Paws Veterinary";
}
