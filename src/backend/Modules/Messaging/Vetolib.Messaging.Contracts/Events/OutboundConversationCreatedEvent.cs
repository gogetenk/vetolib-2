namespace Vetolib.Messaging.Contracts.Events;

public record OutboundConversationCreatedEvent(
    Guid ConversationId,
    Guid OwnerId,
    Guid ClinicId,
    string MessagePreview);
