using FluentAssertions;
using Vetolib.Auth.Application.Commands.CreateUser;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Secure@1a", UserRole.Vet, "VET-12345");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "", "Secure@1a", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "not-an-email", "Secure@1a", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Short_password_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Sh@1a", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 8 characters");
    }

    [Fact]
    public void Password_without_uppercase_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "secure@1abc", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Fact]
    public void Password_without_lowercase_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "SECURE@1ABC", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Fact]
    public void Password_without_digit_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Secure@abcde", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one digit");
    }

    [Fact]
    public void Password_without_special_char_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Secure1abcd", UserRole.Receptionist, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one special character");
    }

    [Fact]
    public void Invalid_role_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Secure@1a", (UserRole)999, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }

    [Fact]
    public void Vet_role_without_license_fails()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "vet@clinic.ae", "Secure@1a", UserRole.Vet, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "VET_LICENSE_REQUIRED");
    }

    [Fact]
    public void Non_vet_role_without_license_passes()
    {
        var cmd = new CreateUserCommand(Guid.NewGuid(), "admin@clinic.ae", "Secure@1a", UserRole.Admin, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
