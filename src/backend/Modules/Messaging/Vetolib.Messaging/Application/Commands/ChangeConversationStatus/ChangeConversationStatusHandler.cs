using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.ChangeConversationStatus;

internal class ChangeConversationStatusHandler : IRequestHandler<ChangeConversationStatusCommand, Result<ConversationDto>>
{
    private readonly MessagingDbContext _context;

    public ChangeConversationStatusHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConversationDto>> Handle(ChangeConversationStatusCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result<ConversationDto>.NotFound();

        var result = cmd.Action switch
        {
            ConversationStatusAction.Resolve => conversation.Resolve(),
            ConversationStatusAction.Close => conversation.Close(),
            ConversationStatusAction.Reopen => conversation.Reopen(),
            _ => Result.Error("Unknown action.")
        };

        if (!result.IsSuccess)
            return Result<ConversationDto>.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<ConversationDto>.Success(conversation.ToDto());
    }
}
