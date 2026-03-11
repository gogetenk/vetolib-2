using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.ListOwnerConversations;

internal class ListOwnerConversationsHandler
    : IRequestHandler<ListOwnerConversationsQuery, Result<IReadOnlyList<ConversationDto>>>
{
    private readonly MessagingDbContext _context;

    public ListOwnerConversationsHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ConversationDto>>> Handle(
        ListOwnerConversationsQuery request,
        CancellationToken cancellationToken)
    {
        // Global query filter applies ClinicId automatically via PortalAwareClinicContext.
        var conversations = await _context.Conversations
            .Where(c => c.OwnerId == request.OwnerId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .AsNoTracking()
            .Select(c => c.ToDto())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ConversationDto>>.Success(conversations);
    }
}
