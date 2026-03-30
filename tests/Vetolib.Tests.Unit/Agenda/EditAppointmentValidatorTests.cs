using FluentAssertions;
using Vetolib.Agenda.Application.Commands.EditAppointment;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class EditAppointmentValidatorTests
{
    private readonly EditAppointmentValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new EditAppointmentCommand(Guid.NewGuid(), null, null, null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_appointment_id_fails()
    {
        var cmd = new EditAppointmentCommand(Guid.Empty, null, null, null, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "AppointmentId is required");
    }

    [Fact]
    public void Empty_guid_veterinarian_id_fails()
    {
        var cmd = new EditAppointmentCommand(Guid.NewGuid(), null, null, null, Guid.Empty, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "VeterinarianId cannot be an empty Guid");
    }

    [Fact]
    public void Zero_duration_fails()
    {
        var cmd = new EditAppointmentCommand(Guid.NewGuid(), null, null, 0, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationMinutes.Value");
    }

    [Fact]
    public void Duration_above_480_fails()
    {
        var cmd = new EditAppointmentCommand(Guid.NewGuid(), null, null, 481, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationMinutes.Value");
    }

    [Fact]
    public void Valid_duration_passes()
    {
        var cmd = new EditAppointmentCommand(Guid.NewGuid(), null, null, 60, null, null, null, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
