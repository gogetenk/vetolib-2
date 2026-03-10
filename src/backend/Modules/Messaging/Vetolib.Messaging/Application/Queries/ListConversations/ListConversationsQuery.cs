using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListConversations;

internal record ListConversationsQuery(
    ConversationStatus? Status,
    MessageCategory? Category,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page,
    int PageSize
) : IRequest<Result<IReadOnlyList<ConversationDto>>>;
