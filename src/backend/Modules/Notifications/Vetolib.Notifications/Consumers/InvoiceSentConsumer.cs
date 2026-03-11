using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Billing.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class InvoiceSentConsumer : IConsumer<InvoiceSentIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IPreferenceChecker _preferenceChecker;
    private readonly ILogger<InvoiceSentConsumer> _logger;

    public InvoiceSentConsumer(
        IEmailSender emailSender,
        IPreferenceChecker preferenceChecker,
        ILogger<InvoiceSentConsumer> logger)
    {
        _emailSender = emailSender;
        _preferenceChecker = preferenceChecker;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InvoiceSentIntegrationEvent> context)
    {
        var evt = context.Message;

        // InvoiceSentIntegrationEvent carries OwnerEmail but no UserId.
        // Per PO decision: preferences apply to clinic staff (Users) only, not owners.
        // Owners always receive invoice emails.

        var message = new EmailMessage(
            To: evt.OwnerEmail,
            Subject: InvoiceEmailTemplate.Subject(evt.InvoiceNumber, evt.ClinicName),
            HtmlBody: InvoiceEmailTemplate.HtmlBody(evt.OwnerName, evt.InvoiceNumber, evt.TotalAmount, evt.Currency, evt.ClinicName),
            PlainTextBody: InvoiceEmailTemplate.PlainTextBody(evt.OwnerName, evt.InvoiceNumber, evt.TotalAmount, evt.Currency, evt.ClinicName));

        var result = await _emailSender.SendAsync(message, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to send invoice email to {Email}: {Errors}",
                evt.OwnerEmail,
                string.Join(", ", result.Errors));

            // Intentional throw: MassTransit retry policy will requeue on transient failures
            throw new InvalidOperationException($"Failed to send invoice email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Invoice email sent to {Email} for invoice {InvoiceNumber}", evt.OwnerEmail, evt.InvoiceNumber);
    }
}
