namespace Vetolib.Messaging.Contracts;

public record ReplyAuditDto(
    Guid Id,
    Guid MessageId,
    Guid OriginalOwnerMessageId,
    string? AiSuggestedReply,
    bool WasSuggestedReplyUsed,
    string ActualReply
);
