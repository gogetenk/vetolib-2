using FluentAssertions;
using Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class UpdateAppointmentStatusValidatorTests
{
    private readonly UpdateAppointmentStatusValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdateAppointmentStatusCommand(Guid.NewGuid(), AppointmentStatus.Completed, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_appointment_id_fails()
    {
        var cmd = new UpdateAppointmentStatusCommand(Guid.Empty, AppointmentStatus.Completed, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "AppointmentId is required");
    }

    [Fact]
    public void Invalid_status_fails()
    {
        var cmd = new UpdateAppointmentStatusCommand(Guid.NewGuid(), (AppointmentStatus)999, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }
}
