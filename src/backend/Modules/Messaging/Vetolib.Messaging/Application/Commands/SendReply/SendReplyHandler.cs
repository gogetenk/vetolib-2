using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.SendReply;

internal class SendReplyHandler : IRequestHandler<SendReplyCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMessagingEventBroadcaster _broadcaster;

    public SendReplyHandler(
        MessagingDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint,
        IMessagingEventBroadcaster broadcaster)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _publishEndpoint = publishEndpoint;
        _broadcaster = broadcaster;
    }

    public async Task<Result<MessageDto>> Handle(SendReplyCommand cmd, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // Determine sender type based on role
        var sender = role == "Vet" ? MessageSender.Vet : MessageSender.Staff;

        // Get senderUserId from sub claim
        Guid? senderUserId = null;
        var subClaim = user?.FindFirst("sub")?.Value
            ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(subClaim, out var parsedId))
            senderUserId = parsedId;

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result<MessageDto>.NotFound();

        var messageResult = conversation.AddMessage(sender, senderUserId, cmd.Body, isInternalNote: false);

        if (!messageResult.IsSuccess)
            return Result<MessageDto>.Error(string.Join("; ", messageResult.Errors));

        await _context.SaveChangesAsync(ct);

        var preview = cmd.Body.Length > 100 ? cmd.Body[..100] + "..." : cmd.Body;
        await _publishEndpoint.Publish(new OwnerMessageReplyEvent(
            conversation.Id,
            conversation.OwnerId,
            conversation.ClinicId,
            preview), ct);

        // Broadcast new-message SSE event to connected staff in the same clinic
        await _broadcaster.BroadcastAsync(new MessagingEvent
        {
            Type = "new-message",
            ClinicId = conversation.ClinicId,
            Category = conversation.Category,
            ConversationId = conversation.Id,
            Preview = preview
        }, ct);

        // Recalculate and broadcast unread-count
        var unreadCount = await _context.Conversations
            .CountAsync(c => c.Status == ConversationStatus.Open && !c.IsSpam, ct);
        await _broadcaster.BroadcastAsync(new MessagingEvent
        {
            Type = "unread-count",
            ClinicId = conversation.ClinicId,
            Category = null,
            UnreadCount = unreadCount
        }, ct);

        return Result<MessageDto>.Success(messageResult.Value.ToDto());
    }
}
