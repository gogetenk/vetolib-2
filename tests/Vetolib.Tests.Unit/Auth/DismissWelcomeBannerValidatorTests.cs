using FluentAssertions;
using Vetolib.Auth.Application.Commands.DismissWelcomeBanner;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DismissWelcomeBannerValidatorTests
{
    private readonly DismissWelcomeBannerValidator _validator = new();

    [Fact]
    public void Validate_WithValidUserId_ReturnsValid()
    {
        var command = new DismissWelcomeBannerCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ReturnsInvalid()
    {
        var command = new DismissWelcomeBannerCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UserId");
        result.Errors[0].ErrorMessage.Should().Be("UserId is required.");
    }
}
