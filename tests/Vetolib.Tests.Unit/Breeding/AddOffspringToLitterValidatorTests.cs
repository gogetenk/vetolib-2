using FluentAssertions;
using Vetolib.Breeding.Application.Commands.AddOffspringToLitter;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class AddOffspringToLitterValidatorTests
{
    private readonly AddOffspringToLitterValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new AddOffspringToLitterCommand(Guid.NewGuid(), Guid.NewGuid(), 1);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_litter_id_fails()
    {
        var cmd = new AddOffspringToLitterCommand(Guid.Empty, Guid.NewGuid(), null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LitterId");
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new AddOffspringToLitterCommand(Guid.NewGuid(), Guid.Empty, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }
}
