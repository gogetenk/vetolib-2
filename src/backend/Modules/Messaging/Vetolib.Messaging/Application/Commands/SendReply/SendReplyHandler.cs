using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.SendReply;

internal class SendReplyHandler : IRequestHandler<SendReplyCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendReplyHandler(MessagingDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
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

        // Note: OwnerMessageReplyEvent would be published via MassTransit when the
        // Notifications.Contracts reference is added in the integration task.
        // For now, the reply is persisted and the owner will be notified by a future wire task.

        return Result<MessageDto>.Success(messageResult.Value.ToDto());
    }
}
