namespace Vetolib.AI.Contracts;

/// <summary>
/// Result of triage performed on an owner message.
/// </summary>
public record MessageTriageResult(
    string Category,
    double Confidence,
    string[] SuggestedReplies,
    string Disclaimer);
