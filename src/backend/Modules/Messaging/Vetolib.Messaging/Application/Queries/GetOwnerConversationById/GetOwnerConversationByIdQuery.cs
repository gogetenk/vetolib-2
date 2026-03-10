using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetOwnerConversationById;

internal record GetOwnerConversationByIdQuery(Guid ConversationId, Guid OwnerId, Guid ClinicId)
    : IRequest<Result<ConversationWithMessagesDto>>;
