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
        // IgnoreQueryFilters because the ClinicId filter is based on JWT which is not set for portal auth.
        // We manually filter by OwnerId and ClinicId from the portal token.
        var conversations = await _context.Conversations
            .IgnoreQueryFilters()
            .Where(c => c.OwnerId == request.OwnerId && c.ClinicId == request.ClinicId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .AsNoTracking()
            .Select(c => c.ToDto())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ConversationDto>>.Success(conversations);
    }
}
