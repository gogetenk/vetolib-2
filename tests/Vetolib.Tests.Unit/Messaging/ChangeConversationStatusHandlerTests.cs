using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.ChangeConversationStatus;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class ChangeConversationStatusHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly IMessagingEventBroadcaster _broadcaster;
    private readonly ChangeConversationStatusHandler _sut;

    public ChangeConversationStatusHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _broadcaster = Substitute.For<IMessagingEventBroadcaster>();
        _sut = new ChangeConversationStatusHandler(_context, _broadcaster);
    }

    [Fact]
    public async Task Handle_Resolve_WhenConversationOpen_ReturnsSuccessWithResolvedStatus()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ChangeConversationStatusCommand(conversation.Id, ConversationStatusAction.Resolve);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ConversationStatus.Resolved);
        await _broadcaster.Received(1).BroadcastAsync(
            Arg.Is<MessagingEvent>(e => e.Type == "conversation-updated" && e.ConversationId == conversation.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Close_ReturnsSuccessWithClosedStatus()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ChangeConversationStatusCommand(conversation.Id, ConversationStatusAction.Close);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ConversationStatus.Closed);
    }

    [Fact]
    public async Task Handle_Reopen_WhenConversationResolved_ReturnsSuccessWithOpenStatus()
    {
        var conversation = CreateConversation();
        conversation.Resolve();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ChangeConversationStatusCommand(conversation.Id, ConversationStatusAction.Reopen);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ConversationStatus.Open);
    }

    [Fact]
    public async Task Handle_Reopen_WhenAlreadyOpen_ReturnsError()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ChangeConversationStatusCommand(conversation.Id, ConversationStatusAction.Reopen);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new ChangeConversationStatusCommand(Guid.NewGuid(), ConversationStatusAction.Resolve);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Escalate_SetsEscalationSentAt()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ChangeConversationStatusCommand(conversation.Id, ConversationStatusAction.Escalate);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
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

    public void Dispose() => _context.Dispose();
}
