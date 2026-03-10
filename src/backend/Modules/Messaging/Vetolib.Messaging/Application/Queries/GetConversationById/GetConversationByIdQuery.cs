using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetConversationById;

internal record GetConversationByIdQuery(Guid ConversationId) : IRequest<Result<ConversationWithMessagesDto>>;
