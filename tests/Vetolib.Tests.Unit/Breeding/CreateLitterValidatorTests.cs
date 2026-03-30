using FluentAssertions;
using Vetolib.Breeding.Application.Commands.CreateLitter;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class CreateLitterValidatorTests
{
    private readonly CreateLitterValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateLitterCommand(Guid.NewGuid(), Guid.NewGuid(), null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), 4, 4, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_mother_patient_id_fails()
    {
        var cmd = new CreateLitterCommand(Guid.NewGuid(), Guid.Empty, null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), 4, 4, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MotherPatientId");
    }

    [Fact]
    public void Negative_born_count_fails()
    {
        var cmd = new CreateLitterCommand(Guid.NewGuid(), Guid.NewGuid(), null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), -1, 0, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BornCount");
    }

    [Fact]
    public void Negative_alive_count_fails()
    {
        var cmd = new CreateLitterCommand(Guid.NewGuid(), Guid.NewGuid(), null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), 4, -1, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AliveCount");
    }
}
