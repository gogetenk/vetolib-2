namespace Vetolib.Messaging.Application.Commands.CreateOwnerConversation;

internal record CreateOwnerConversationResponse(
    Guid Id,
    string EstimatedResponseTime,
    string? AssignedToRole,
    string? Acknowledgment = null);
