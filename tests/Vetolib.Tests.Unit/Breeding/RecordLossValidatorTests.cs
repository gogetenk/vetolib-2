using FluentAssertions;
using Vetolib.Breeding.Application.Commands.RecordLoss;
using Vetolib.Breeding.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class RecordLossValidatorTests
{
    private readonly RecordLossValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RecordLossCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.Miscarriage, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_pregnancy_id_fails()
    {
        var cmd = new RecordLossCommand(Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.Miscarriage, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PregnancyId");
    }

    [Fact]
    public void Invalid_outcome_fails()
    {
        var cmd = new RecordLossCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow),
            (PregnancyOutcome)999, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Outcome");
    }
}
