namespace Vetolib.Messaging.Contracts.Events;

public record OwnerMessageReplyEvent(
    Guid ConversationId,
    Guid OwnerId,
    Guid ClinicId,
    string ReplyPreview);
