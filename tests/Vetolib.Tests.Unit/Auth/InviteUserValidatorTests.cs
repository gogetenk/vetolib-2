using FluentAssertions;
using Vetolib.Auth.Application.Commands.InviteUser;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class InviteUserValidatorTests
{
    private readonly InviteUserValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new InviteUserCommand(Guid.NewGuid(), Guid.NewGuid(), "vet@clinic.ae", "Ahmed Al Maktoum", UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var cmd = new InviteUserCommand(Guid.NewGuid(), Guid.NewGuid(), "", "Ahmed", UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var cmd = new InviteUserCommand(Guid.NewGuid(), Guid.NewGuid(), "not-email", "Ahmed", UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Empty_full_name_fails()
    {
        var cmd = new InviteUserCommand(Guid.NewGuid(), Guid.NewGuid(), "vet@clinic.ae", "", UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void FullName_exceeding_200_chars_fails()
    {
        var cmd = new InviteUserCommand(Guid.NewGuid(), Guid.NewGuid(), "vet@clinic.ae", new string('A', 201), UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new InviteUserCommand(Guid.Empty, Guid.NewGuid(), "vet@clinic.ae", "Ahmed", UserRole.Vet);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }
}
