using Ardalis.Result;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Auth.Application.Commands.InviteVet;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class InviteVetHandlerTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly AuthDbContext _context;
    private readonly InviteVetHandler _handler;

    public InviteVetHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(Guid.NewGuid());

        var publisher = Substitute.For<MediatR.IPublisher>();
        _context = new AuthDbContext(options, clinicContext, publisher);

        _handler = new InviteVetHandler(
            _context,
            _emailSender,
            NullLogger<InviteVetHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidRequest_SendsEmailAndLogsInvitation()
    {
        // Arrange
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var command = new InviteVetCommand(
            "dr.hamdan@desertpaws.ae", "Fatima Al-Rashidi", "Luna", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == "dr.hamdan@desertpaws.ae"),
            Arg.Any<CancellationToken>());

        var logs = await _context.VetInvitationLogs.IgnoreQueryFilters().ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].VetEmail.Should().Be("dr.hamdan@desertpaws.ae");
        logs[0].OwnerName.Should().Be("Fatima Al-Rashidi");
        logs[0].PetName.Should().Be("Luna");
    }

    [Fact]
    public async Task Handle_WithCustomMessage_IncludesMessageInEmail()
    {
        // Arrange
        EmailMessage? capturedMessage = null;
        _emailSender.SendAsync(Arg.Do<EmailMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var command = new InviteVetCommand(
            "vet@clinic.ae", "Ahmed", "Buddy", "Please join, you are the best vet!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.HtmlBody.Should().Contain("Please join, you are the best vet!");
        capturedMessage.PlainTextBody.Should().Contain("Please join, you are the best vet!");
    }

    [Fact]
    public async Task Handle_RateLimitExceeded_ReturnsConflict()
    {
        // Arrange — seed 3 existing invitations for today
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var targetEmail = "busy.vet@clinic.ae";

        for (var i = 0; i < InviteVetHandler.MaxInvitationsPerEmailPerDay; i++)
        {
            var cmd = new InviteVetCommand(targetEmail, $"Owner{i}", $"Pet{i}", null);
            await _handler.Handle(cmd, CancellationToken.None);
        }

        // Act — 4th invitation should be rate limited
        var result = await _handler.Handle(
            new InviteVetCommand(targetEmail, "ExtraOwner", "ExtraPet", null),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_EmailSendFails_DoesNotPersistLog()
    {
        // Arrange
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("SMTP connection refused"));

        var command = new InviteVetCommand("vet@test.ae", "Sara", "Max", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        var logs = await _context.VetInvitationLogs.IgnoreQueryFilters().ToListAsync();
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmailNormalized_ToLowercase()
    {
        // Arrange
        EmailMessage? capturedMessage = null;
        _emailSender.SendAsync(Arg.Do<EmailMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var command = new InviteVetCommand("Dr.Vet@CLINIC.AE", "Maryam", "Rex", null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.To.Should().Be("dr.vet@clinic.ae");

        var log = await _context.VetInvitationLogs.IgnoreQueryFilters().FirstAsync();
        log.VetEmail.Should().Be("dr.vet@clinic.ae");
    }

    [Fact]
    public void BuildEmail_ContainsPitchContent()
    {
        // Act
        var email = InviteVetHandler.BuildEmail("vet@test.ae", "Khalid", "Simba", null);

        // Assert
        email.Subject.Should().Contain("Khalid");
        email.Subject.Should().Contain("Vetara");
        email.PlainTextBody.Should().Contain("Simba");
        email.PlainTextBody.Should().Contain("Vetara");
        email.PlainTextBody.Should().Contain("free trial");
        email.HtmlBody.Should().Contain("AI-powered health alerts");
        email.HtmlBody.Should().Contain("Online booking");
        email.HtmlBody.Should().Contain("Shared medical records");
        email.HtmlBody.Should().Contain("https://app.vetara.ae/register");
    }
}
