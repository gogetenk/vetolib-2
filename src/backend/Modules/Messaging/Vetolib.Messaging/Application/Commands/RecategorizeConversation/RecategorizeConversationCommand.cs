using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.RecategorizeConversation;

internal record RecategorizeConversationCommand(
    Guid ConversationId,
    MessageCategory NewCategory
) : IRequest<Result<ConversationDto>>;
