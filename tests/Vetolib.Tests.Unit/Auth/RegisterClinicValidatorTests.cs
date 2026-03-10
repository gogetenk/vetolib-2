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
        var cmd = new RegisterClinicCommand("Desert Paws", "owner@test.ae", "Secure@1234567!", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Blank_clinic_name_fails()
    {
        var cmd = new RegisterClinicCommand("", "owner@test.ae", "Secure@1234567!", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicName");
    }

    [Fact]
    public void Short_password_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "short", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Password_without_special_char_fails()
    {
        var cmd = new RegisterClinicCommand("Test Clinic", "owner@test.ae", "NoSpecialChar1234", "+971501234567", "AE");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password",
            "NoSpecialChar1234 has no special characters");
    }
}
