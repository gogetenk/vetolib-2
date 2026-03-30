using FluentAssertions;
using Vetolib.Breeding.Application.Commands.ScheduleCheck;
using Vetolib.Breeding.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class ScheduleCheckValidatorTests
{
    private readonly ScheduleCheckValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new ScheduleCheckCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PregnancyCheckType.Ultrasound, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_pregnancy_id_fails()
    {
        var cmd = new ScheduleCheckCommand(Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PregnancyCheckType.Ultrasound, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PregnancyId");
    }

    [Fact]
    public void Invalid_check_type_fails()
    {
        var cmd = new ScheduleCheckCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            (PregnancyCheckType)999, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CheckType");
    }
}
