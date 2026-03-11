using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.AddInternalNote;

internal class AddInternalNoteHandler : IRequestHandler<AddInternalNoteCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddInternalNoteHandler(MessagingDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<MessageDto>> Handle(AddInternalNoteCommand cmd, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var sender = role == "Vet" ? MessageSender.Vet : MessageSender.Staff;

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
        var messageResult = conversation.AddMessage(sender, senderUserId, cmd.Body, isInternalNote: true);

        if (!messageResult.IsSuccess)
            return Result<MessageDto>.Invalid(messageResult.ValidationErrors);

        _context.Messages.Add(messageResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<MessageDto>.Success(messageResult.Value.ToDto());
    }
}
