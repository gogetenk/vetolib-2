using System.Security.Claims;
using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.OverrideClassification;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class OverrideClassificationHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly OverrideClassificationHandler _sut;

    public OverrideClassificationHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserId.ToString()),
            new Claim(ClaimTypes.Role, "Vet")
        ]));
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _sut = new OverrideClassificationHandler(_context, _httpContextAccessor);
    }

    [Fact]
    public async Task Handle_WhenMessageExistsAndClassified_OverridesClassification()
    {
        // Arrange
        var conversation = CreateConversation();
        var message = AddClassifiedMessage(conversation);
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new OverrideClassificationCommand(
            conversation.Id, message.Id,
            ClassifiedUrgency.Critical, ClassifiedCategory.MedicalConcern);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Messages.FindAsync(message.Id);
        updated!.ClassifiedUrgency.Should().Be(ClassifiedUrgency.Critical);
        updated.ClassifiedCategory.Should().Be(ClassifiedCategory.MedicalConcern);
        updated.OverriddenByUserId.Should().Be(UserId);
        updated.OriginalAiUrgency.Should().Be(ClassifiedUrgency.Normal);
        updated.OriginalAiCategory.Should().Be(ClassifiedCategory.AdministrativeRequest);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new OverrideClassificationCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenMessageNotFound_ReturnsNotFound()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new OverrideClassificationCommand(
            conversation.Id, Guid.NewGuid(),
            ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenMessageNotClassified_ReturnsError()
    {
        var conversation = CreateConversation();
        var messageResult = Message.Create(conversation.Id, MessageSender.Owner, null, "unclassified msg");
        var message = messageResult.Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new OverrideClassificationCommand(
            conversation.Id, message.Id,
            ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("NOT_CLASSIFIED"));
    }

    private static Conversation CreateConversation()
    {
        return Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            null,
            "Test conversation",
            MessageCategory.Administrative).Value;
    }

    private static Message AddClassifiedMessage(Conversation conversation)
    {
        var messageResult = Message.Create(conversation.Id, MessageSender.Owner, null, "Test message body");
        var message = messageResult.Value;
        message.ApplyClassification(
            ClassifiedUrgency.Normal,
            ClassifiedCategory.AdministrativeRequest,
            0.85,
            false);
        return message;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
