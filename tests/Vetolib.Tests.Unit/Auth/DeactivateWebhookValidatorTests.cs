using FluentAssertions;
using Vetolib.Auth.Application.Commands.DeactivateWebhook;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DeactivateWebhookValidatorTests
{
    private readonly DeactivateWebhookValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new DeactivateWebhookCommand(Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_id_fails()
    {
        var cmd = new DeactivateWebhookCommand(Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
