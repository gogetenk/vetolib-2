using Ardalis.Result;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Application.Domain;
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
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result<MessageDto>.NotFound();

        // Create message via domain, add directly to DbSet to avoid
        // EF Core collection tracking issue with Include(Messages).
        var messageResult = conversation.AddMessage(sender, senderUserId, cmd.Body, isInternalNote: false);

        if (!messageResult.IsSuccess)
            return Result<MessageDto>.Error(string.Join("; ", messageResult.Errors));

        var replyMessage = messageResult.Value;
        _context.Messages.Add(replyMessage);

        // Process attachments from PendingUploads
        if (cmd.AttachmentIds is { Count: > 0 })
        {
            var pendingUploads = await _context.PendingUploads
                .Where(p => cmd.AttachmentIds.Contains(p.Id))
                .ToListAsync(ct);

            if (pendingUploads.Count != cmd.AttachmentIds.Count)
                return Result<MessageDto>.NotFound("One or more attachment IDs not found or expired");

            foreach (var pending in pendingUploads)
            {
                var attachResult = replyMessage.AddAttachment(
                    pending.FileName,
                    pending.ContentType,
                    pending.FileSizeBytes,
                    pending.StoragePath);

                if (!attachResult.IsSuccess)
                    return Result<MessageDto>.Error(string.Join("; ", attachResult.Errors));

                _context.MessageAttachments.Add(attachResult.Value);
                _context.PendingUploads.Remove(pending);
            }
        }

        // Find the last owner message to reference in the audit
        var lastOwnerMessage = await _context.Messages
            .Where(m => m.ConversationId == cmd.ConversationId && m.Sender == MessageSender.Owner)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync(ct);

        // Record ReplyAudit for AI suggestion effectiveness tracking
        if (lastOwnerMessage is not null)
        {
            var auditResult = ReplyAudit.Create(
                replyMessage.Id,
                lastOwnerMessage.Id,
                cmd.Body,
                cmd.AiSuggestedReply,
                cmd.WasSuggestedReplyUsed);

            if (auditResult.IsSuccess)
                _context.ReplyAudits.Add(auditResult.Value);
        }

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

        return Result<MessageDto>.Success(replyMessage.ToDto());
    }
}
