using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.TransferConversation;

internal class TransferConversationHandler : IRequestHandler<TransferConversationCommand, Result<ConversationDto>>
{
    private readonly MessagingDbContext _context;

    public TransferConversationHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConversationDto>> Handle(TransferConversationCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result<ConversationDto>.NotFound();

        var result = conversation.AssignTo(cmd.AssignedToUserId, cmd.AssignedToRole);

        if (!result.IsSuccess)
            return Result<ConversationDto>.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<ConversationDto>.Success(conversation.ToDto());
    }
}
