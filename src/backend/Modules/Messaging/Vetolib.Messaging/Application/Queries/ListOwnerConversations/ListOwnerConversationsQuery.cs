using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListOwnerConversations;

internal record ListOwnerConversationsQuery(Guid OwnerId, Guid ClinicId, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<ConversationPagedResultDto>>;
