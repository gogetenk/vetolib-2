using MediatR;

namespace Vetolib.Messaging.Contracts.Events;

public record UrgentMessageClassifiedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid ClinicId,
    ClassifiedUrgency Urgency,
    string MessagePreview
) : INotification;
