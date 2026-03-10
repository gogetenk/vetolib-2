namespace Vetolib.Messaging.Contracts;

public record OutboundConversationRequest(
    Guid OwnerId,
    Guid? PatientId,
    string Subject,
    string InitialMessageBody,
    MessageCategory Category = MessageCategory.Administrative);
