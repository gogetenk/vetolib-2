using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.AddWeightEntry;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddWeightEntryValidatorTests
{
    private readonly AddWeightEntryValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 5.5m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.Empty, Guid.NewGuid(), 5.5m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.Empty, 5.5m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Zero_weight_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 0m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Weight must be greater than zero");
    }

    [Fact]
    public void Negative_weight_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), -1m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Weight must be greater than zero");
    }

    [Fact]
    public void Weight_exceeding_10000_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 10001m, "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Weight exceeds maximum allowed value");
    }

    [Fact]
    public void Empty_recorded_by_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 5.5m, "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "RecordedBy is required");
    }

    [Fact]
    public void Note_exceeding_500_chars_fails()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 5.5m, "Dr. Ahmed", new string('A', 501));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Note");
    }

    [Fact]
    public void Null_note_passes()
    {
        var cmd = new AddWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 5.5m, "Dr. Ahmed", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
