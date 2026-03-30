using FluentAssertions;
using Vetolib.Agenda.Application.Commands.CreateConsultationType;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateConsultationTypeValidatorTests
{
    private readonly CreateConsultationTypeValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.NewGuid(), "General Checkup", 30);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.Empty, "General Checkup", 30);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "ClinicId is required");
    }

    [Fact]
    public void Empty_name_fails()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.NewGuid(), "", 30);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public void Name_exceeding_100_chars_fails()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.NewGuid(), new string('A', 101), 30);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name must not exceed 100 characters");
    }

    [Fact]
    public void Duration_below_10_fails()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.NewGuid(), "Checkup", 5);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duration must be between 10 and 180 minutes");
    }

    [Fact]
    public void Duration_above_180_fails()
    {
        var cmd = new CreateConsultationTypeCommand(Guid.NewGuid(), "Checkup", 200);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duration must be between 10 and 180 minutes");
    }
}
