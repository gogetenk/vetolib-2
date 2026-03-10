using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.ChangeConversationStatus;

internal record ChangeConversationStatusCommand(
    Guid ConversationId,
    ConversationStatusAction Action
) : IRequest<Result<ConversationDto>>;
