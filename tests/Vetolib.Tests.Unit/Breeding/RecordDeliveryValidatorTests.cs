using FluentAssertions;
using Vetolib.Breeding.Application.Commands.RecordDelivery;
using Vetolib.Breeding.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class RecordDeliveryValidatorTests
{
    private readonly RecordDeliveryValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RecordDeliveryCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.LiveBirth, 4, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_pregnancy_id_fails()
    {
        var cmd = new RecordDeliveryCommand(Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.LiveBirth, 4, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PregnancyId");
    }

    [Fact]
    public void Invalid_outcome_fails()
    {
        var cmd = new RecordDeliveryCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow),
            (PregnancyOutcome)999, 4, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Outcome");
    }

    [Fact]
    public void Negative_offspring_count_fails()
    {
        var cmd = new RecordDeliveryCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow),
            PregnancyOutcome.LiveBirth, -1, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OffspringCount");
    }
}
