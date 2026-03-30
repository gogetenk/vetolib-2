using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;
using Vetolib.Messaging.Application.Commands.SendReply;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class SendReplyHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMessagingEventBroadcaster _broadcaster;
    private readonly SendReplyHandler _sut;

    public SendReplyHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserId.ToString()),
            new Claim(ClaimTypes.Role, "Vet")
        ]));
        httpContextAccessor.HttpContext.Returns(httpContext);

        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _broadcaster = Substitute.For<IMessagingEventBroadcaster>();

        _sut = new SendReplyHandler(_context, httpContextAccessor, _publishEndpoint, _broadcaster);
    }

    [Fact]
    public async Task Handle_WhenConversationExists_AddsMessageAndReturnsSuccess()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new SendReplyCommand(conversation.Id, "Your pet is doing well.");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Body.Should().Be("Your pet is doing well.");
        result.Value.Sender.Should().Be(MessageSender.Vet);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new SendReplyCommand(Guid.NewGuid(), "Reply body");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_PublishesOwnerMessageReplyEvent()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new SendReplyCommand(conversation.Id, "Reply content");

        await _sut.Handle(cmd, CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Any<Vetolib.Messaging.Contracts.Events.OwnerMessageReplyEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BroadcastsNewMessageAndUnreadCountEvents()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new SendReplyCommand(conversation.Id, "Reply body");

        await _sut.Handle(cmd, CancellationToken.None);

        await _broadcaster.Received(1).BroadcastAsync(
            Arg.Is<MessagingEvent>(e => e.Type == "new-message"),
            Arg.Any<CancellationToken>());
        await _broadcaster.Received(1).BroadcastAsync(
            Arg.Is<MessagingEvent>(e => e.Type == "unread-count"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClosedConversation_ReturnsError()
    {
        var conversation = CreateConversation();
        conversation.Close();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new SendReplyCommand(conversation.Id, "Reply to closed");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithAiSuggestedReply_CreatesReplyAuditWhenOwnerMessageExists()
    {
        var conversation = CreateConversation();
        // Add an owner message first
        var ownerMsg = Message.Create(conversation.Id, MessageSender.Owner, null, "Help my cat").Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(ownerMsg);
        await _context.SaveChangesAsync();

        var cmd = new SendReplyCommand(
            conversation.Id,
            "Your cat needs a checkup",
            AiSuggestedReply: "AI suggested reply",
            WasSuggestedReplyUsed: true);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var audits = await _context.ReplyAudits.ToListAsync();
        audits.Should().HaveCount(1);
        audits[0].WasSuggestedReplyUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithStaffRole_SetsSenderAsStaff()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserId.ToString()),
            new Claim(ClaimTypes.Role, "Receptionist")
        ]));
        httpContextAccessor.HttpContext.Returns(httpContext);

        var sut = new SendReplyHandler(context, httpContextAccessor,
            Substitute.For<IPublishEndpoint>(), Substitute.For<IMessagingEventBroadcaster>());

        var conversation = CreateConversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var cmd = new SendReplyCommand(conversation.Id, "Staff reply");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sender.Should().Be(MessageSender.Staff);

        context.Dispose();
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
