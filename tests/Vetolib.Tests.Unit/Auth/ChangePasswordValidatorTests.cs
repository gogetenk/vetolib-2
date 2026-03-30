using FluentAssertions;
using Vetolib.Auth.Application.Commands.ChangePassword;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewSecur@1a");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_current_password_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "", "NewSecur@1a");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
    }

    [Fact]
    public void Empty_new_password_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public void Short_new_password_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "Sh@1a");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 8 characters");
    }

    [Fact]
    public void New_password_without_uppercase_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "newpassword@1");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Fact]
    public void New_password_without_lowercase_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NEWPASSWORD@1");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Fact]
    public void New_password_without_digit_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewPassword@ab");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one digit");
    }

    [Fact]
    public void New_password_without_special_char_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewPassword1a");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one special character");
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        var cmd = new ChangePasswordCommand(Guid.Empty, "OldPassword1!", "NewSecur@1a");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
