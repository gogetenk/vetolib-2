using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.CreateOutboundConversation;

internal record CreateOutboundConversationCommand(
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    string InitialMessageBody,
    MessageCategory Category = MessageCategory.Administrative
) : IRequest<Result<ConversationDto>>;
