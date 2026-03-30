using FluentAssertions;
using Vetolib.Auth.Application.Commands.VerifyEmail;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class VerifyEmailValidatorTests
{
    private readonly VerifyEmailValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_IsValid()
    {
        var command = new VerifyEmailCommand("valid-token-value");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullToken_IsInvalid(string? token)
    {
        var command = new VerifyEmailCommand(token!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Token");
    }
}
