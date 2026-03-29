using FluentAssertions;
using Vetolib.Breeding.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class HeatCyclePredictionTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    private static HeatCycle CreateCycle(DateOnly start, DateOnly? end = null)
        => HeatCycle.Create(ClinicId, PatientId, start, end).Value;

    [Fact]
    public void Predict_WithLessThan2Cycles_ShouldFail()
    {
        var cycles = new List<HeatCycle>
        {
            CreateCycle(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 25))
        };

        var result = HeatCyclePrediction.Predict(cycles);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("At least 2 heat cycles"));
    }

    [Fact]
    public void Predict_WithEmptyList_ShouldFail()
    {
        var result = HeatCyclePrediction.Predict(new List<HeatCycle>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Predict_With2Cycles_ShouldSucceedWithoutConfidence()
    {
        var cycles = new List<HeatCycle>
        {
            CreateCycle(new DateOnly(2025, 7, 10)),
            CreateCycle(new DateOnly(2026, 1, 10))
        };

        var result = HeatCyclePrediction.Predict(cycles);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasConfidence.Should().BeFalse();
        result.Value.AverageCycleIntervalDays.Should().Be(184); // July 10 -> Jan 10 = 184 days
        result.Value.PredictedNextStart.Should().Be(new DateOnly(2026, 1, 10).AddDays(184));
    }

    [Fact]
    public void Predict_With3Cycles_ShouldSucceedWithConfidence()
    {
        var cycles = new List<HeatCycle>
        {
            CreateCycle(new DateOnly(2025, 1, 15)),
            CreateCycle(new DateOnly(2025, 7, 10)),
            CreateCycle(new DateOnly(2026, 1, 10))
        };

        var result = HeatCyclePrediction.Predict(cycles);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasConfidence.Should().BeTrue();

        // Interval 1: Jan 15 -> Jul 10 = 176 days
        // Interval 2: Jul 10 -> Jan 10 = 184 days
        // Average: (176 + 184) / 2 = 180 days
        result.Value.AverageCycleIntervalDays.Should().Be(180);
        result.Value.PredictedNextStart.Should().Be(new DateOnly(2026, 7, 9)); // Jan 10 + 180 = Jul 9
    }

    [Fact]
    public void Predict_ShouldOrderCyclesChronologically()
    {
        // Pass cycles in reverse order to ensure they're sorted internally
        var cycles = new List<HeatCycle>
        {
            CreateCycle(new DateOnly(2026, 1, 10)),
            CreateCycle(new DateOnly(2025, 1, 15)),
            CreateCycle(new DateOnly(2025, 7, 10))
        };

        var result = HeatCyclePrediction.Predict(cycles);

        result.IsSuccess.Should().BeTrue();
        result.Value.AverageCycleIntervalDays.Should().Be(180);
    }
}
