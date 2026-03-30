using Ardalis.Result;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// MailKit-based SMTP email sender with Polly Circuit Breaker and Retry.
/// Replaces the System.Net.Mail implementation for better
/// async support and compatibility with MailHog / production SMTP.
/// </summary>
internal class MailKitEmailSender : IEmailSender
{
    private readonly MailKitSmtpOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;
    private readonly ResiliencePipeline _pipeline;

    public MailKitEmailSender(IOptions<MailKitSmtpOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            // Timeout: 5s per attempt
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5)
            })
            // Retry: 2 retries with exponential backoff (1s, 2s)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(ex => ex is not OperationCanceledException and not BrokenCircuitException)
            })
            // Circuit Breaker: opens after 3 failures, stays open 30s
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(ex => ex is not OperationCanceledException),
                OnOpened = args =>
                {
                    _logger.LogWarning("Email circuit breaker OPENED. SMTP unavailable for {Duration}s.",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    _logger.LogInformation("Email circuit breaker CLOSED. SMTP service resumed.");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token =>
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

                await client.ConnectAsync(_options.Host, _options.Port, socketOptions, token);

                if (!string.IsNullOrWhiteSpace(_options.Username))
                    await client.AuthenticateAsync(_options.Username, _options.Password, token);

                await client.SendAsync(mimeMessage, token);
                await client.DisconnectAsync(quit: true, token);

                _logger.LogInformation("Email sent to {To} with subject '{Subject}'", message.To, message.Subject);

                return Result.Success();
            }, ct);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Email circuit breaker is open — cannot send to {To}", message.To);
            return Result.Error("Email service temporarily unavailable");
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning(ex, "Email send timed out for {To}", message.To);
            return Result.Error($"Email send timed out: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
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
    public string FromAddress { get; set; } = "noreply@vetara.ae";
    public string FromName { get; set; } = "Vetara";
}
