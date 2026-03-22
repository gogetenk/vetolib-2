namespace Vetolib.Messaging.Contracts;

/// <summary>
/// Classifies individual messages by urgency and category using AI.
/// </summary>
public interface IMessageClassifier
{
    /// <summary>
    /// Classifies a message text and returns urgency, category, and confidence.
    /// Returns null if the service is unavailable (circuit breaker open, AI down).
    /// </summary>
    Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default);
}

public record MessageClassificationResult(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category,
    double Confidence
);
