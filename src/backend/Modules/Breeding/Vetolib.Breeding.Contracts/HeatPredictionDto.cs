namespace Vetolib.Breeding.Contracts;

public record HeatPredictionDto(
    DateOnly PredictedNextStart,
    int AverageCycleIntervalDays,
    bool HasConfidence);
