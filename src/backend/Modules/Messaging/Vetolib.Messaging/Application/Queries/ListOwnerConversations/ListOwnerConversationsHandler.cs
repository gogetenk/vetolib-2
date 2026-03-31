using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.ListOwnerConversations;

internal class ListOwnerConversationsHandler
    : IRequestHandler<ListOwnerConversationsQuery, Result<ConversationPagedResultDto>>
{
    private readonly MessagingDbContext _context;

    public ListOwnerConversationsHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConversationPagedResultDto>> Handle(
        ListOwnerConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);

        // ClinicId is handled by the global query filter via PortalAwareClinicContext.
        var baseQuery = _context.Conversations
            .Where(c => c.OwnerId == request.OwnerId)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var conversations = await baseQuery
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => c.ToDto())
            .ToListAsync(cancellationToken);

        return Result<ConversationPagedResultDto>.Success(
            new ConversationPagedResultDto(conversations, totalCount, page, pageSize));
    }
}
