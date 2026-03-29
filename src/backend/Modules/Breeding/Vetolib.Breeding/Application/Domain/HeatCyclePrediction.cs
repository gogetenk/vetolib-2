using Ardalis.Result;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Domain;

internal static class HeatCyclePrediction
{
    public static Result<HeatPredictionDto> Predict(IReadOnlyList<HeatCycle> cycles)
    {
        if (cycles.Count < 2)
            return Result<HeatPredictionDto>.Error("At least 2 heat cycles are required to make a prediction");

        var ordered = cycles.OrderBy(c => c.StartDate).ToList();

        var intervals = new List<int>();
        for (var i = 1; i < ordered.Count; i++)
        {
            var interval = ordered[i].StartDate.DayNumber - ordered[i - 1].StartDate.DayNumber;
            intervals.Add(interval);
        }

        var avgInterval = (int)Math.Round(intervals.Average());
        var lastStart = ordered[^1].StartDate;
        var predictedNext = lastStart.AddDays(avgInterval);
        var hasConfidence = cycles.Count >= 3;

        return Result<HeatPredictionDto>.Success(
            new HeatPredictionDto(predictedNext, avgInterval, hasConfidence));
    }
}
