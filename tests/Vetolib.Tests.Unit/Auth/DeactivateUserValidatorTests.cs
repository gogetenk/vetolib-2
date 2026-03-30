using FluentAssertions;
using Vetolib.Auth.Application.Commands.DeactivateUser;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DeactivateUserValidatorTests
{
    private readonly DeactivateUserValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new DeactivateUserCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_requesting_user_id_fails()
    {
        var cmd = new DeactivateUserCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "RequestingUserId is required");
    }

    [Fact]
    public void Empty_target_user_id_fails()
    {
        var cmd = new DeactivateUserCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "TargetUserId is required");
    }
}
