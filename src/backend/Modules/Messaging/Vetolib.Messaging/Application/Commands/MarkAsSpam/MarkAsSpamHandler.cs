using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.MarkAsSpam;

internal class MarkAsSpamHandler : IRequestHandler<MarkAsSpamCommand, Result>
{
    private readonly MessagingDbContext _context;

    public MarkAsSpamHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsSpamCommand cmd, CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result.NotFound();

        var result = conversation.MarkAsSpam();

        if (!result.IsSuccess)
            return Result.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
