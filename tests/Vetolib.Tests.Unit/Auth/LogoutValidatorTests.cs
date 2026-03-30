using FluentAssertions;
using Vetolib.Auth.Application.Commands.Logout;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class LogoutValidatorTests
{
    private readonly LogoutValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new LogoutCommand(Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        var cmd = new LogoutCommand(Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserId is required");
    }
}
