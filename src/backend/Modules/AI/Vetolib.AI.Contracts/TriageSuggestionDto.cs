namespace Vetolib.AI.Contracts;

public record TriageSuggestionDto(
    Guid TriageId,
    AISeverity Severity,
    int EstimatedDurationMinutes,
    string RecommendedSpecialty,
    string Reasoning,
    string Disclaimer,
    double Confidence);
