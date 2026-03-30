using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class AppointmentCancellationConsumerTests
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentCancellationConsumer> _logger;
    private readonly AppointmentCancellationConsumer _consumer;

    public AppointmentCancellationConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AppointmentCancellationConsumer>.Instance;
        _consumer = new AppointmentCancellationConsumer(_emailSender, _logger);
    }

    private static ConsumeContext<AppointmentCancelledIntegrationEvent> BuildContext(AppointmentCancelledIntegrationEvent evt)
    {
        var context = Substitute.For<ConsumeContext<AppointmentCancelledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static AppointmentCancelledIntegrationEvent BuildEvent() => new()
    {
        OwnerEmail = "fatima.al-zaabi@example.com",
        OwnerName = "Fatima Al-Zaabi",
        PatientName = "Luna",
        VetName = "Dr. Ahmed",
        ScheduledAt = new DateTime(2026, 4, 1, 10, 30, 0),
        ClinicName = "Abu Dhabi Animal Care",
        CancellationReason = "Vet unavailable"
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

    [Fact]
    public async Task Consume_WithArabicLanguage_SendsArabicSubject()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent() with { PreferredLanguage = "ar" };
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.Subject.Contains("تم إلغاء الموعد")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WithEnglishLanguage_SendsEnglishSubject()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent() with { PreferredLanguage = "en" };
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.Subject.Contains("Appointment cancelled")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WithCancellationReason_IncludesReasonInBody()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent() with { CancellationReason = "Vet unavailable" };
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.HtmlBody.Contains("Vet unavailable")),
            Arg.Any<CancellationToken>());
    }
}
