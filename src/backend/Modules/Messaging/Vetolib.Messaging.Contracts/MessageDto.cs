namespace Vetolib.Messaging.Contracts;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    MessageSender Sender,
    Guid? SenderUserId,
    string Body,
    bool IsInternalNote,
    DateTime SentAt
);
