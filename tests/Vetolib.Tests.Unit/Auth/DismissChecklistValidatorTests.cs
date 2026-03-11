using FluentAssertions;
using Vetolib.Auth.Application.Commands.DismissChecklist;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DismissChecklistValidatorTests
{
    private readonly DismissChecklistValidator _validator = new();

    [Fact]
    public void Validate_WithValidUserId_ReturnsValid()
    {
        var command = new DismissChecklistCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ReturnsInvalid()
    {
        var command = new DismissChecklistCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UserId");
        result.Errors[0].ErrorMessage.Should().Be("UserId is required.");
    }
}
