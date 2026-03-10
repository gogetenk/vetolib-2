namespace Vetolib.Messaging.Contracts.Events;

public record EmergencyMessageReceivedEvent(
    Guid ConversationId,
    Guid ClinicId,
    Guid? PatientId,
    string MessagePreview);
