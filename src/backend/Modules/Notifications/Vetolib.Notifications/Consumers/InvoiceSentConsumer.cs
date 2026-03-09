using MassTransit;
using Microsoft.Extensions.Logging;
using Vetolib.Billing.Contracts;
using Vetolib.Notifications.Templates;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Consumers;

internal class InvoiceSentConsumer : IConsumer<InvoiceSentIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InvoiceSentConsumer> _logger;

    public InvoiceSentConsumer(IEmailSender emailSender, ILogger<InvoiceSentConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InvoiceSentIntegrationEvent> context)
    {
        var evt = context.Message;

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

            // INFRA: MassTransit retry mechanism requires exception propagation — not business control flow
            throw new InvalidOperationException($"Failed to send invoice email to {evt.OwnerEmail}");
        }

        _logger.LogInformation("Invoice email sent to {Email} for invoice {InvoiceNumber}", evt.OwnerEmail, evt.InvoiceNumber);
    }
}
