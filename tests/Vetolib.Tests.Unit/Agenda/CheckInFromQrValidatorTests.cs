using FluentAssertions;
using Vetolib.Agenda.Application.Commands.CheckInFromQr;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CheckInFromQrValidatorTests
{
    private readonly CheckInFromQrValidator _validator = new();

    private static CheckInFromQrCommand ValidCommand() => new(
        Guid.NewGuid(),
        "Buddy",
        "Mohammed Al Rashid",
        DateTime.UtcNow.AddHours(1),
        Guid.NewGuid(),
        "valid-hmac-signature");

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_appointment_id_fails()
    {
        var cmd = ValidCommand() with { AppointmentId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AppointmentId");
    }

    [Fact]
    public void Empty_patient_name_fails()
    {
        var cmd = ValidCommand() with { PatientName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientName");
    }

    [Fact]
    public void Empty_owner_name_fails()
    {
        var cmd = ValidCommand() with { OwnerName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OwnerName");
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = ValidCommand() with { ClinicId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Empty_signature_fails()
    {
        var cmd = ValidCommand() with { Signature = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Signature");
    }

    [Fact]
    public void Null_signature_fails()
    {
        var cmd = ValidCommand() with { Signature = null! };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Signature");
    }
}
