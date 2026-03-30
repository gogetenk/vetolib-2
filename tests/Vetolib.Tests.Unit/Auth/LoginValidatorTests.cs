using FluentAssertions;
using Vetolib.Auth.Application.Commands.Login;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new LoginCommand("user@clinic.ae", "Password123!");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var cmd = new LoginCommand("", "Password123!");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var cmd = new LoginCommand("not-an-email", "Password123!");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Empty_password_fails()
    {
        var cmd = new LoginCommand("user@clinic.ae", "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
