using FluentAssertions;
using Vetolib.Auth.Application.Commands.DismissWelcomeBanner;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DismissWelcomeBannerValidatorTests
{
    private readonly DismissWelcomeBannerValidator _validator = new();

    [Fact]
    public void Valid_userId_passes()
    {
        var cmd = new DismissWelcomeBannerCommand(Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_userId_fails()
    {
        var cmd = new DismissWelcomeBannerCommand(Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
