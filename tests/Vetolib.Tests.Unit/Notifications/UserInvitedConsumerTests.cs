using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Auth.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class UserInvitedConsumerTests
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserInvitedConsumer> _logger;
    private readonly UserInvitedConsumer _consumer;

    public UserInvitedConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<UserInvitedConsumer>.Instance;
        _consumer = new UserInvitedConsumer(_emailSender, Substitute.For<IPreferenceChecker>(), _logger);
    }

    private static ConsumeContext<UserInvitedIntegrationEvent> BuildContext(UserInvitedIntegrationEvent evt)
    {
        var context = Substitute.For<ConsumeContext<UserInvitedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static UserInvitedIntegrationEvent BuildEvent() => new()
    {
        Email = "khalid.al-nuaimi@vetoclinic.ae",
        FullName = "Khalid Al-Nuaimi",
        TemporaryPassword = "Temp@Pass123!",
        ClinicName = "Sharjah Animal Hospital"
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
    public async Task Consume_WhenEmailSendSucceeds_SendsEmailToInvitedUserAddress()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent();
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == evt.Email),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_ThrowsInvalidOperationException()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Relay access denied"));

        var context = BuildContext(BuildEvent());

        var act = async () => await _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khalid.al-nuaimi@vetoclinic.ae*");
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_StillCallsEmailSenderOnce()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Service unavailable"));

        var context = BuildContext(BuildEvent());

        try { await _consumer.Consume(context); } catch { /* expected */ }

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }
}
