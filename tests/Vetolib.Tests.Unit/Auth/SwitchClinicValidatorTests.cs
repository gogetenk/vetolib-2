using FluentAssertions;
using Vetolib.Auth.Application.Commands.SwitchClinic;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class SwitchClinicValidatorTests
{
    private readonly SwitchClinicValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new SwitchClinicCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        var cmd = new SwitchClinicCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserId is required");
    }

    [Fact]
    public void Empty_target_clinic_id_fails()
    {
        var cmd = new SwitchClinicCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "TargetClinicId is required");
    }
}
