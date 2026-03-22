using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.OverrideClassification;

internal class OverrideClassificationHandler : IRequestHandler<OverrideClassificationCommand, Result>
{
    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OverrideClassificationHandler(
        MessagingDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(OverrideClassificationCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result.NotFound("Conversation not found");

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == cmd.MessageId && m.ConversationId == cmd.ConversationId, ct);

        if (message is null)
            return Result.NotFound("Message not found");

        var user = _httpContextAccessor.HttpContext?.User;
        var subClaim = user?.FindFirst("sub")?.Value
            ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(subClaim, out var userId))
            return Result.Error("INVALID_USER:Unable to determine current user");

        var overrideResult = message.OverrideClassification(userId, cmd.Urgency, cmd.Category);
        if (!overrideResult.IsSuccess)
            return overrideResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
