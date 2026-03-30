using FluentAssertions;
using FluentValidation;
using Vetolib.Auth.Application.Commands.RegisterClinic;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RegisterClinicValidatorTests
{
    private readonly RegisterClinicValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new RegisterClinicCommand("Desert Paws", "owner@test.ae", "Secure@1a", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Blank_clinic_name_fails()
    {
        var cmd = new RegisterClinicCommand("", "owner@test.ae", "Secure@1a", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicName");
    }

    [Fact]
    public void Short_password_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "Sh@1a", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Password_without_special_char_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "NoSpecial1a", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one special character");
    }

    [Fact]
    public void Password_without_uppercase_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "nouppercase@1", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Fact]
    public void Password_without_lowercase_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "NOLOWERCASE@1", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Fact]
    public void Password_without_digit_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "NoDigits@abc", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one digit");
    }
}
