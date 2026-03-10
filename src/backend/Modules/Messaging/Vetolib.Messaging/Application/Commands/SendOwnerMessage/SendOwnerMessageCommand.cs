using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.SendOwnerMessage;

internal record SendOwnerMessageCommand(
    Guid ConversationId,
    Guid OwnerId,
    Guid ClinicId,
    string Body
) : IRequest<Result<Guid>>;
