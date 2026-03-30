using FluentAssertions;
using Vetolib.Breeding.Application.Commands.CompleteCheck;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class CompleteCheckValidatorTests
{
    private readonly CompleteCheckValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CompleteCheckCommand(Guid.NewGuid(), "All good");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_check_id_fails()
    {
        var cmd = new CompleteCheckCommand(Guid.Empty, "All good");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CheckId");
    }
}
