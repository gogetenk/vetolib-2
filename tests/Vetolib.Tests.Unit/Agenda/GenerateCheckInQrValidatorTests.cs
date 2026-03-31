using FluentAssertions;
using Vetolib.Agenda.Application.Commands.GenerateCheckInQr;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class GenerateCheckInQrValidatorTests
{
    private readonly GenerateCheckInQrValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new GenerateCheckInQrCommand(Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_appointment_id_fails()
    {
        var cmd = new GenerateCheckInQrCommand(Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AppointmentId");
    }
}
