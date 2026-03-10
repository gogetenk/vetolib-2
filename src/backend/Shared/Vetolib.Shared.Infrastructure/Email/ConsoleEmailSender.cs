using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure.Email;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[EMAIL] To: {To} | Subject: {Subject}\n{Body}",
            message.To,
            message.Subject,
            message.PlainTextBody ?? message.HtmlBody);

        return Task.FromResult(Result.Success());
    }
}
