using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.TransferConversation;

internal record TransferConversationCommand(
    Guid ConversationId,
    Guid? AssignedToUserId,
    string? AssignedToRole
) : IRequest<Result<ConversationDto>>;
