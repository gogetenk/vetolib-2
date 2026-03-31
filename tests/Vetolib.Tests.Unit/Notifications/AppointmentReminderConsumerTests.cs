using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Notifications.Contracts;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Contracts.Events;
using Vetolib.Notifications.Domain;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class AppointmentReminderConsumerTests : IDisposable
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsProvider _smsProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AppointmentReminderConsumer> _logger;
    private readonly NotificationsDbContext _dbContext;
    private readonly AppointmentReminderConsumer _consumer;

    private static readonly Guid TestClinicId = new("22222222-2222-2222-2222-222222222222");

    public AppointmentReminderConsumerTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _smsProvider = Substitute.For<ISmsProvider>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AppointmentReminderConsumer>.Instance;

        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(TestClinicId);
        _dbContext = new NotificationsDbContext(options, clinicContext, Substitute.For<IPublisher>());

        _consumer = new AppointmentReminderConsumer(
            _emailSender,
            _smsProvider,
            Substitute.For<IPreferenceChecker>(),
            _publishEndpoint,
            _dbContext,
            _logger);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static ConsumeContext<AppointmentReminderDueIntegrationEvent> BuildContext(AppointmentReminderDueIntegrationEvent evt)
    {
        var context = Substitute.For<ConsumeContext<AppointmentReminderDueIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static AppointmentReminderDueIntegrationEvent BuildEvent(
        Guid? clinicId = null,
        string ownerPhone = "") => new()
    {
        OwnerEmail = "ahmed.al-rashidi@example.com",
        OwnerName = "Ahmed Al-Rashidi",
        PatientName = "Baxter",
        VetName = "Dr. Sarah Al-Mansoori",
        ScheduledAt = new DateTime(2026, 3, 12, 10, 30, 0),
        ClinicName = "Dubai Veterinary Clinic",
        ClinicId = clinicId ?? TestClinicId,
        OwnerPhone = ownerPhone
    };

    private async Task SeedReminderConfig(Guid clinicId, ReminderChannel channel)
    {
        var config = ReminderConfig.CreateDefault(clinicId).Value;
        config.Update(true, true, true, 24, 7, channel);
        _dbContext.ReminderConfigs.Add(config);
        await _dbContext.SaveChangesAsync();
    }

    // --- Existing email tests (updated for new constructor) ---

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
    public async Task Consume_WhenEmailSendSucceeds_LogsReminderAsSent()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var context = BuildContext(BuildEvent());

        await _consumer.Consume(context);

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].DeliveryStatus.Should().Be(DeliveryStatus.Sent);
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

    [Fact]
    public async Task Consume_WhenEmailSendFails_LogsReminderAsFailed()
    {
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMTP timeout"));

        var context = BuildContext(BuildEvent());

        try { await _consumer.Consume(context); } catch { /* expected */ }

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].DeliveryStatus.Should().Be(DeliveryStatus.Failed);
    }

    // --- Channel routing tests ---

    [Fact]
    public async Task ResolveChannel_WhenNoConfig_ReturnsEmail()
    {
        var channel = await _consumer.ResolveChannelAsync(TestClinicId, CancellationToken.None);

        channel.Should().Be(ReminderChannel.Email);
    }

    [Fact]
    public async Task ResolveChannel_WhenEmptyClinicId_ReturnsEmail()
    {
        var channel = await _consumer.ResolveChannelAsync(Guid.Empty, CancellationToken.None);

        channel.Should().Be(ReminderChannel.Email);
    }

    [Fact]
    public async Task ResolveChannel_WhenConfigIsWhatsApp_ReturnsWhatsApp()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.WhatsApp);

        var channel = await _consumer.ResolveChannelAsync(TestClinicId, CancellationToken.None);

        channel.Should().Be(ReminderChannel.WhatsApp);
    }

    [Fact]
    public async Task ResolveChannel_WhenConfigIsBoth_ReturnsBoth()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Both);

        var channel = await _consumer.ResolveChannelAsync(TestClinicId, CancellationToken.None);

        channel.Should().Be(ReminderChannel.Both);
    }

    [Fact]
    public async Task Consume_WhenChannelIsWhatsApp_DoesNotSendEmail()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.WhatsApp);

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsWhatsApp_PublishesWhatsAppEvent()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.WhatsApp);

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<SendWhatsAppReminderEvent>(e =>
                e.OwnerPhone == "+971501234567" &&
                e.OwnerName == "Ahmed Al-Rashidi" &&
                e.PatientName == "Baxter"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsBoth_SendsEmailAndPublishesWhatsApp()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Both);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<SendWhatsAppReminderEvent>(e => e.OwnerPhone == "+971501234567"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsWhatsApp_ButNoPhone_SkipsWhatsApp()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.WhatsApp);

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<SendWhatsAppReminderEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsEmail_DoesNotPublishWhatsApp()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Email);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<SendWhatsAppReminderEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsBoth_LogsBothChannels()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Both);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(2);
        logs.Should().Contain(l => l.Channel == NotificationChannel.Email);
        logs.Should().Contain(l => l.Channel == NotificationChannel.Sms); // WhatsApp logged as Sms
    }

    // --- SMS channel tests ---

    [Fact]
    public async Task Consume_WhenChannelIsSms_CallsSmsProvider()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _smsProvider.Received(1).SendSmsAsync(
            "+971501234567",
            Arg.Is<string>(m => m.Contains("Baxter")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsSms_DoesNotSendEmail()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsSms_DoesNotPublishWhatsApp()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<SendWhatsAppReminderEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsSms_ButNoPhone_SkipsSms()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _smsProvider.DidNotReceive().SendSmsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsSms_LogsSmsChannel()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Channel.Should().Be(NotificationChannel.Sms);
        logs[0].DeliveryStatus.Should().Be(DeliveryStatus.Sent);
    }

    [Fact]
    public async Task Consume_WhenChannelIsSmsAndSmsFails_ThrowsInvalidOperationException()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMS gateway unreachable"));

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        var act = async () => await _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*+971501234567*");
    }

    [Fact]
    public async Task Consume_WhenChannelIsSmsAndSmsFails_LogsSmsAsFailed()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMS gateway unreachable"));

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        try { await _consumer.Consume(context); } catch { /* expected */ }

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].DeliveryStatus.Should().Be(DeliveryStatus.Failed);
    }

    [Fact]
    public async Task Consume_WhenChannelIsAll_SendsEmailWhatsAppAndSms()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.All);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<SendWhatsAppReminderEvent>(e => e.OwnerPhone == "+971501234567"),
            Arg.Any<CancellationToken>());

        await _smsProvider.Received(1).SendSmsAsync(
            "+971501234567",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenChannelIsAll_LogsAllThreeChannels()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.All);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _smsProvider.SendSmsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = BuildEvent(clinicId: TestClinicId, ownerPhone: "+971501234567");
        var context = BuildContext(evt);

        await _consumer.Consume(context);

        var logs = await _dbContext.ReminderLogs.ToListAsync();
        logs.Should().HaveCount(3);
        logs.Should().Contain(l => l.Channel == NotificationChannel.Email);
        logs.Should().Contain(l => l.Channel == NotificationChannel.Sms);
    }

    [Fact]
    public async Task ResolveChannel_WhenConfigIsSms_ReturnsSms()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.Sms);

        var channel = await _consumer.ResolveChannelAsync(TestClinicId, CancellationToken.None);

        channel.Should().Be(ReminderChannel.Sms);
    }

    [Fact]
    public async Task ResolveChannel_WhenConfigIsAll_ReturnsAll()
    {
        await SeedReminderConfig(TestClinicId, ReminderChannel.All);

        var channel = await _consumer.ResolveChannelAsync(TestClinicId, CancellationToken.None);

        channel.Should().Be(ReminderChannel.All);
    }
}
