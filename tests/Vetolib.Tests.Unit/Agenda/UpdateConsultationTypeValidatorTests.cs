using FluentAssertions;
using Vetolib.Agenda.Application.Commands.UpdateConsultationType;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class UpdateConsultationTypeValidatorTests
{
    private readonly UpdateConsultationTypeValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), "Surgery", 60, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_id_fails()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.Empty, "Surgery", 60, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Id is required");
    }

    [Fact]
    public void Empty_name_fails()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), "", 60, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public void Name_exceeding_100_chars_fails()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), new string('A', 101), 60, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name must not exceed 100 characters");
    }

    [Fact]
    public void Duration_below_10_fails()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), "Surgery", 5, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duration must be between 10 and 180 minutes");
    }

    [Fact]
    public void Duration_above_180_fails()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), "Surgery", 200, 1, false);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duration must be between 10 and 180 minutes");
    }
}
