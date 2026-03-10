namespace Vetolib.AI.Contracts;

/// <summary>
/// AI service that summarizes long conversation threads for staff.
/// The summary is factual (3-5 sentences) and never includes medical diagnoses.
/// </summary>
public interface IConversationSummaryService
{
    /// <summary>
    /// Produces a factual summary of the conversation messages.
    /// Returns null if the AI service is unavailable or fails.
    /// </summary>
    Task<string?> SummarizeAsync(
        IReadOnlyList<string> messages,
        CancellationToken cancellationToken = default);
}
