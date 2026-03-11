using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Billing.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class InvoiceSentConsumerTests
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InvoiceSentConsumer> _logger;
    private readonly InvoiceSentConsumer _consumer;

    public InvoiceSentConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<InvoiceSentConsumer>.Instance;
        _consumer = new InvoiceSentConsumer(_emailSender, Substitute.For<IPreferenceChecker>(), _logger);
    }

    private static ConsumeContext<InvoiceSentIntegrationEvent> BuildContext(InvoiceSentIntegrationEvent evt)
    {
        var context = Substitute.For<ConsumeContext<InvoiceSentIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static InvoiceSentIntegrationEvent BuildEvent() => new()
    {
        OwnerEmail = "fatima.al-zaabi@example.com",
        OwnerName = "Fatima Al-Zaabi",
        InvoiceNumber = "INV-2026-0042",
        TotalAmount = 350.00m,
        Currency = "AED",
        ClinicName = "Abu Dhabi Animal Care"
    };

    [Fact]
    public async Task Consume_WhenEmailSendSucceeds_DoesNotThrow()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var context = BuildContext(BuildEvent());

        var act = async () => await _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WhenEmailSendSucceeds_CallsEmailSenderOnce()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var context = BuildContext(BuildEvent());

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenEmailSendSucceeds_SendsEmailToOwnerAddress()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent();
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == evt.OwnerEmail),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_ThrowsInvalidOperationException()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Mailbox unavailable"));

        var context = BuildContext(BuildEvent());

        var act = async () => await _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*fatima.al-zaabi@example.com*");
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_StillCallsEmailSenderOnce()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Network error"));

        var context = BuildContext(BuildEvent());

        try { await _consumer.Consume(context); } catch { /* expected */ }

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }
}
