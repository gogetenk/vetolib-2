using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.TransferConversation;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class TransferConversationHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly IMessagingEventBroadcaster _broadcaster;
    private readonly TransferConversationHandler _sut;

    public TransferConversationHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _broadcaster = Substitute.For<IMessagingEventBroadcaster>();
        _sut = new TransferConversationHandler(_context, _broadcaster);
    }

    [Fact]
    public async Task Handle_WhenConversationExists_AssignsUserAndRole()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var targetUserId = Guid.NewGuid();
        var cmd = new TransferConversationCommand(conversation.Id, targetUserId, "Vet");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssignedToUserId.Should().Be(targetUserId);
        result.Value.AssignedToRole.Should().Be("Vet");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new TransferConversationCommand(Guid.NewGuid(), Guid.NewGuid(), "Vet");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_BroadcastsConversationUpdatedEvent()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new TransferConversationCommand(conversation.Id, Guid.NewGuid(), "Vet");

        await _sut.Handle(cmd, CancellationToken.None);

        await _broadcaster.Received(1).BroadcastAsync(
            Arg.Is<MessagingEvent>(e => e.Type == "conversation-updated" && e.ConversationId == conversation.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullUserIdAndRole_UnassignsConversation()
    {
        var conversation = CreateConversation();
        conversation.AssignTo(Guid.NewGuid(), "Vet");
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new TransferConversationCommand(conversation.Id, null, null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssignedToUserId.Should().BeNull();
        result.Value.AssignedToRole.Should().BeNull();
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
