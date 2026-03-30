using FluentAssertions;
using Vetolib.Auth.Application.Commands.ChangeUserRole;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ChangeUserRoleValidatorTests
{
    private readonly ChangeUserRoleValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new ChangeUserRoleCommand(Guid.NewGuid(), Guid.NewGuid(), UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_requesting_user_id_fails()
    {
        var cmd = new ChangeUserRoleCommand(Guid.Empty, Guid.NewGuid(), UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RequestingUserId");
    }

    [Fact]
    public void Empty_target_user_id_fails()
    {
        var cmd = new ChangeUserRoleCommand(Guid.NewGuid(), Guid.Empty, UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TargetUserId");
    }

    [Fact]
    public void Invalid_role_fails()
    {
        var cmd = new ChangeUserRoleCommand(Guid.NewGuid(), Guid.NewGuid(), (UserRole)999);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewRole");
    }
}
