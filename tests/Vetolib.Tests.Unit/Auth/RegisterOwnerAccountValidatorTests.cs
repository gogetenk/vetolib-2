using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Auth.Application.Commands.RegisterOwnerAccount;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RegisterOwnerAccountValidatorTests
{
    private readonly RegisterOwnerAccountValidator _validator = new();

    [Fact]
    public void Valid_Command_NoErrors()
    {
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Fatima Al Rashid", "SecureP@ss1");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_HasError()
    {
        var command = new RegisterOwnerAccountCommand("", "+971501234567", "Fatima", "SecureP@ss1");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_Email_HasError()
    {
        var command = new RegisterOwnerAccountCommand("not-an-email", "+971501234567", "Fatima", "SecureP@ss1");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_Phone_HasError()
    {
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "", "Fatima", "SecureP@ss1");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Empty_FullName_HasError()
    {
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "", "SecureP@ss1");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Empty_Password_HasError()
    {
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Fatima", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Short_Password_HasError()
    {
        var command = new RegisterOwnerAccountCommand("fatima@example.com", "+971501234567", "Fatima", "1234567");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
