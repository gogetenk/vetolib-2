using FluentAssertions;
using Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RemoveClinicFromGroupValidatorTests
{
    private readonly RemoveClinicFromGroupValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RemoveClinicFromGroupCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_group_id_fails()
    {
        var cmd = new RemoveClinicFromGroupCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "GroupId is required");
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new RemoveClinicFromGroupCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "ClinicId is required");
    }
}
