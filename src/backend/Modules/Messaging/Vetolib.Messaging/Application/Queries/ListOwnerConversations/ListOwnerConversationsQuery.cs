using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListOwnerConversations;

internal record ListOwnerConversationsQuery(Guid OwnerId, Guid ClinicId)
    : IRequest<Result<IReadOnlyList<ConversationDto>>>;
