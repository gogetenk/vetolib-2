namespace Vetolib.Messaging.Contracts;

public record ConversationTransferRequest(Guid? AssignedToUserId, string? AssignedToRole);
