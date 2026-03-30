using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.CreateOwner;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreateOwnerValidatorTests
{
    private readonly CreateOwnerValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateOwnerCommand(Guid.NewGuid(), "Mohammed", "Al Rashid", "mohammed@example.ae", "+971501234567");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_first_name_fails()
    {
        var cmd = new CreateOwnerCommand(Guid.NewGuid(), "", "Al Rashid", "mohammed@example.ae", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
    }

    [Fact]
    public void Empty_last_name_fails()
    {
        var cmd = new CreateOwnerCommand(Guid.NewGuid(), "Mohammed", "", "mohammed@example.ae", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var cmd = new CreateOwnerCommand(Guid.NewGuid(), "Mohammed", "Al Rashid", "not-an-email", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Empty_email_fails()
    {
        var cmd = new CreateOwnerCommand(Guid.NewGuid(), "Mohammed", "Al Rashid", "", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
