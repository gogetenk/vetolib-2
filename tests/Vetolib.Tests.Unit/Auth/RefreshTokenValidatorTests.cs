using FluentAssertions;
using Vetolib.Auth.Application.Commands.RefreshToken;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RefreshTokenCommand("valid-refresh-token-string");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_refresh_token_fails()
    {
        var cmd = new RefreshTokenCommand("");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Refresh token is required");
    }
}
