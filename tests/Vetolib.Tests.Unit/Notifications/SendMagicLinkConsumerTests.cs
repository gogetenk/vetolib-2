using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Notifications.Consumers;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class SendMagicLinkConsumerTests
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendMagicLinkConsumer> _logger;
    private readonly SendMagicLinkConsumer _consumer;

    private static readonly Guid OwnerId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    public SendMagicLinkConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SendMagicLinkConsumer>.Instance;
        _consumer = new SendMagicLinkConsumer(_emailSender, _logger);
    }

    private static ConsumeContext<SendMagicLinkEvent> BuildContext(SendMagicLinkEvent evt)
    {
        var context = Substitute.For<ConsumeContext<SendMagicLinkEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static SendMagicLinkEvent BuildEvent() => new(
        OwnerId: OwnerId,
        ClinicId: ClinicId,
        OwnerEmail: "mariam.al-falasi@example.com",
        PortalUrl: "https://portal.vetolib.ae/login?token=abc123");

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
            .Returns(Result.Error("SMTP auth failed"));

        var context = BuildContext(BuildEvent());

        var act = async () => await _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*mariam.al-falasi@example.com*");
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_StillCallsEmailSenderOnce()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Connection timeout"));

        var context = BuildContext(BuildEvent());

        try { await _consumer.Consume(context); } catch { /* expected */ }

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }
}
