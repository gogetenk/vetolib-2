using FluentAssertions;
using Vetolib.Auth.Application.Commands.RegisterWebhook;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RegisterWebhookValidatorTests
{
    private readonly RegisterWebhookValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "super-secret-key-1234567890", ["lab.result"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var cmd = new RegisterWebhookCommand("", "super-secret-key-1234567890", ["lab.result"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Empty_secret_fails()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "", ["lab.result"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Secret");
    }

    [Fact]
    public void Short_secret_fails()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "short", ["lab.result"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Secret");
    }

    [Fact]
    public void Empty_event_types_fails()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "super-secret-key-1234567890", []);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventTypes");
    }

    [Fact]
    public void Invalid_event_type_fails()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "super-secret-key-1234567890", ["invalid.type"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("EventTypes"));
    }

    [Fact]
    public void Multiple_valid_event_types_pass()
    {
        var cmd = new RegisterWebhookCommand("IDEXX", "super-secret-key-1234567890", ["lab.result", "external.record"]);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
