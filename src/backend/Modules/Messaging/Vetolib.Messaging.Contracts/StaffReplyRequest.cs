namespace Vetolib.Messaging.Contracts;

public record StaffReplyRequest(
    string Body,
    string? AiSuggestedReply = null,
    bool WasSuggestedReplyUsed = false,
    List<Guid>? AttachmentIds = null);
