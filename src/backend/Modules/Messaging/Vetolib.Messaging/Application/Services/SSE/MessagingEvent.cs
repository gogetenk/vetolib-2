using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services.SSE;

internal sealed record MessagingEvent
{
    public required string Type { get; init; }
    public required Guid ClinicId { get; init; }
    public required MessageCategory? Category { get; init; }
    public Guid? ConversationId { get; init; }
    public string? Preview { get; init; }
    public ConversationStatus? NewStatus { get; init; }
    public int? UnreadCount { get; init; }

    /// <summary>
    /// Whether this event is medical in nature (MedicalUrgency, PostOperativeFollowUp, MedicalQuestion).
    /// Used to filter out medical events from receptionist connections.
    /// </summary>
    public bool IsMedical => Category is
        MessageCategory.MedicalUrgency or
        MessageCategory.PostOperativeFollowUp or
        MessageCategory.MedicalQuestion;
}
