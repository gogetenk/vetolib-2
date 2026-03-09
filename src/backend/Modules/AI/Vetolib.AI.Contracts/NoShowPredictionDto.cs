namespace Vetolib.AI.Contracts;

public record NoShowPredictionDto(
    Guid AppointmentId,
    float NoShowProbability,
    string RiskLevel,
    List<string> TopFactors,
    List<string> Suggestions);
