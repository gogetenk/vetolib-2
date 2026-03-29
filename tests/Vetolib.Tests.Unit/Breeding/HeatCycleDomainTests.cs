using FluentAssertions;
using Vetolib.Breeding.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class HeatCycleDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 25);

        var result = HeatCycle.Create(ClinicId, PatientId, start, end, "Good signs");

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.StartDate.Should().Be(start);
        result.Value.EndDate.Should().Be(end);
        result.Value.Notes.Should().Be("Good signs");
    }

    [Fact]
    public void Create_WithoutEndDate_ShouldSucceed()
    {
        var start = new DateOnly(2026, 1, 10);

        var result = HeatCycle.Create(ClinicId, PatientId, start);

        result.IsSuccess.Should().BeTrue();
        result.Value.EndDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldFail()
    {
        var start = new DateOnly(2026, 3, 15);
        var end = new DateOnly(2026, 3, 10);

        var result = HeatCycle.Create(ClinicId, PatientId, start, end);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public void Create_WithEndDateEqualToStartDate_ShouldFail()
    {
        var start = new DateOnly(2026, 3, 15);

        var result = HeatCycle.Create(ClinicId, PatientId, start, start);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ShouldFail()
    {
        var result = HeatCycle.Create(Guid.Empty, PatientId, new DateOnly(2026, 1, 10));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ShouldFail()
    {
        var result = HeatCycle.Create(ClinicId, Guid.Empty, new DateOnly(2026, 1, 10));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void ToDto_ShouldComputeDurationDays()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 25);
        var cycle = HeatCycle.Create(ClinicId, PatientId, start, end).Value;

        var dto = cycle.ToDto();

        dto.DurationDays.Should().Be(15);
    }

    [Fact]
    public void ToDto_WithoutEndDate_ShouldHaveNullDurationDays()
    {
        var cycle = HeatCycle.Create(ClinicId, PatientId, new DateOnly(2026, 1, 10)).Value;

        var dto = cycle.ToDto();

        dto.DurationDays.Should().BeNull();
    }

    [Fact]
    public void Create_WithNotes_ShouldTrimWhitespace()
    {
        var result = HeatCycle.Create(ClinicId, PatientId, new DateOnly(2026, 1, 10), notes: "  Some note  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be("Some note");
    }
}
