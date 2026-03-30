using FluentAssertions;
using Vetolib.Breeding.Application.Commands.CreatePregnancy;
using Vetolib.Breeding.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class CreatePregnancyValidatorTests
{
    private readonly CreatePregnancyValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreatePregnancyCommand(Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow), MatingMethod.Natural, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new CreatePregnancyCommand(Guid.Empty, null,
            DateOnly.FromDateTime(DateTime.UtcNow), MatingMethod.Natural, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Invalid_mating_method_fails()
    {
        var cmd = new CreatePregnancyCommand(Guid.NewGuid(), null,
            DateOnly.FromDateTime(DateTime.UtcNow), (MatingMethod)999, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MatingMethod");
    }
}
