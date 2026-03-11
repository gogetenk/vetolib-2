using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class AppointmentReminderConsumerTests
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentReminderConsumer> _logger;
    private readonly AppointmentReminderConsumer _consumer;

    public AppointmentReminderConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AppointmentReminderConsumer>.Instance;
        _consumer = new AppointmentReminderConsumer(_emailSender, Substitute.For<IPreferenceChecker>(), _logger);
    }

    private static ConsumeContext<AppointmentReminderDueIntegrationEvent> BuildContext(AppointmentReminderDueIntegrationEvent evt)
    {
        var context = Substitute.For<ConsumeContext<AppointmentReminderDueIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static AppointmentReminderDueIntegrationEvent BuildEvent() => new()
    {
        OwnerEmail = "ahmed.al-rashidi@example.com",
        OwnerName = "Ahmed Al-Rashidi",
        PatientName = "Baxter",
        VetName = "Dr. Sarah Al-Mansoori",
        ScheduledAt = new DateTime(2026, 3, 12, 10, 30, 0),
        ClinicName = "Dubai Veterinary Clinic"
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
            Arg.Is<EmailMessage>(m => m.To == "ahmed.al-rashidi@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenEmailSendSucceeds_SendsEmailWithOwnerAddress()
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
            .Returns(Result.Error("SMTP connection refused"));

        var context = BuildContext(BuildEvent());

        var act = async () => await _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ahmed.al-rashidi@example.com*");
    }

    [Fact]
    public async Task Consume_WhenEmailSendFails_StillCallsEmailSenderOnce()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMTP timeout"));

        var context = BuildContext(BuildEvent());

        try { await _consumer.Consume(context); } catch { /* expected */ }

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }
}
