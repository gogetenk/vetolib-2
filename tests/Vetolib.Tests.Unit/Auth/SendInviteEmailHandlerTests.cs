using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Auth.Contracts;
using Vetolib.Notifications.Consumers;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

/// <summary>
/// Tests for UserInvitedConsumer — the MassTransit consumer that sends invitation emails.
/// Replaces the deleted SendInviteEmailHandler (which was a direct MediatR notification handler).
/// </summary>
public class SendInviteEmailHandlerTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IPreferenceChecker _preferenceChecker = Substitute.For<IPreferenceChecker>();
    private readonly UserInvitedConsumer _consumer;

    public SendInviteEmailHandlerTests()
    {
        // Default: preferences allow sending
        _preferenceChecker.IsTrueAsync(Arg.Any<Guid>(), Arg.Any<PreferenceKey>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        _consumer = new UserInvitedConsumer(
            _emailSender,
            _preferenceChecker,
            NullLogger<UserInvitedConsumer>.Instance);
    }

    private static ConsumeContext<UserInvitedIntegrationEvent> BuildContext(UserInvitedIntegrationEvent evt)
    {
        var ctx = Substitute.For<ConsumeContext<UserInvitedIntegrationEvent>>();
        ctx.Message.Returns(evt);
        ctx.CancellationToken.Returns(CancellationToken.None);
        return ctx;
    }

    [Fact]
    public async Task Consume_SendsEmailToCorrectRecipient()
    {
        // Arrange
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = new UserInvitedIntegrationEvent
        {
            Email = "dr.hamdan@desertpaws.ae",
            FullName = "Dr. Hamdan Al Maktoum",
            ClinicName = "Desert Paws Veterinary Clinic"
        };

        // Act
        await _consumer.Consume(BuildContext(evt));

        // Assert
        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == "dr.hamdan@desertpaws.ae"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_SubjectContainsClinicName()
    {
        // Arrange
        EmailMessage? capturedMessage = null;
        _emailSender.SendAsync(Arg.Do<EmailMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = new UserInvitedIntegrationEvent
        {
            Email = "fatima.al-rashidi@desertpaws.ae",
            FullName = "Fatima Al-Rashidi",
            ClinicName = "Desert Paws Veterinary Clinic"
        };

        // Act
        await _consumer.Consume(BuildContext(evt));

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Subject.Should().Contain("Desert Paws Veterinary Clinic");
    }

    [Fact]
    public async Task Consume_BodyDoesNotContainPlaintextPassword()
    {
        // Arrange
        EmailMessage? capturedMessage = null;
        _emailSender.SendAsync(Arg.Do<EmailMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var evt = new UserInvitedIntegrationEvent
        {
            Email = "youssef.khalil@alain.ae",
            FullName = "Youssef Khalil",
            ClinicName = "Al Ain Animal Hospital"
        };

        // Act
        await _consumer.Consume(BuildContext(evt));

        // Assert — email should instruct user to use Forgot Password, not contain a plaintext password
        capturedMessage.Should().NotBeNull();
        capturedMessage!.PlainTextBody.Should().Contain("Forgot Password");
        capturedMessage.HtmlBody.Should().Contain("Forgot Password");
    }

    [Fact]
    public async Task Consume_WhenEmailFails_ThrowsForMassTransitRetry()
    {
        // Arrange — consumer should throw to trigger MassTransit retry
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMTP connection refused"));

        var evt = new UserInvitedIntegrationEvent
        {
            Email = "aisha.mansouri@test.ae",
            FullName = "Aisha Mansouri",
            ClinicName = "Dubai Pet Care"
        };

        // Act — must throw so MassTransit can retry
        var act = async () => await _consumer.Consume(BuildContext(evt));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
