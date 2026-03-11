using FluentAssertions;
using Vetolib.Auth.Application.Commands.DismissChecklist;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DismissChecklistValidatorTests
{
    private readonly DismissChecklistValidator _validator = new();

    [Fact]
    public void Valid_userId_passes()
    {
        var cmd = new DismissChecklistCommand(Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_userId_fails()
    {
        var cmd = new DismissChecklistCommand(Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
