using FluentAssertions;
using Vetolib.Breeding.Application.Commands.RecordHeatCycle;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class RecordHeatCycleValidatorTests
{
    private readonly RecordHeatCycleValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RecordHeatCycleCommand(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new RecordHeatCycleCommand(Guid.Empty, Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new RecordHeatCycleCommand(Guid.NewGuid(), Guid.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }
}
