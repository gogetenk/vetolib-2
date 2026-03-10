namespace Vetolib.AI.Contracts;

/// <summary>
/// AI service that triages an owner message and optionally generates suggested replies.
/// Implementations must be gracefully skippable (return null on failure).
/// </summary>
public interface IMessageTriageService
{
    /// <summary>
    /// Triages a message and returns category, confidence, and suggested replies.
    /// Returns null if the AI service is unavailable or fails.
    /// </summary>
    Task<MessageTriageResult?> TriageAsync(
        string messageContent,
        string? additionalContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates suggested replies for a staff member viewing a conversation.
    /// Returns an empty array if the AI service is unavailable or fails.
    /// </summary>
    Task<string[]> GenerateSuggestedRepliesAsync(
        string conversationSubject,
        string lastOwnerMessage,
        string category,
        CancellationToken cancellationToken = default);
}
