namespace Vetolib.Messaging.Contracts.Events;

public record EmergencyEscalationEvent(
    Guid ConversationId,
    Guid ClinicId,
    string MessagePreview);
