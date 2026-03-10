using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.CreateOwnerConversation;

internal record CreateOwnerConversationCommand(
    Guid OwnerId,
    Guid ClinicId,
    Guid? PatientId,
    string Subject,
    MessageCategory Category,
    string Body
) : IRequest<Result<Guid>>;
