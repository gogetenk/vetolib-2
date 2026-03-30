using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddMedicalRecordValidatorTests
{
    private readonly AddMedicalRecordValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "Dermatitis", "Topical cream", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_patient_id_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.Empty, "Dermatitis", "Topical cream", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.Empty, Guid.NewGuid(), "Dermatitis", "Topical cream", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Empty_diagnosis_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "", "Topical cream", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Diagnosis");
    }

    [Fact]
    public void Diagnosis_exceeding_1000_chars_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), new string('A', 1001), "Topical cream", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Diagnosis");
    }

    [Fact]
    public void Empty_treatment_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "Dermatitis", "", "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Treatment");
    }

    [Fact]
    public void Treatment_exceeding_2000_chars_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "Dermatitis", new string('A', 2001), "Dr. Ahmed");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Treatment");
    }

    [Fact]
    public void Empty_vet_name_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "Dermatitis", "Topical cream", "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VetName");
    }

    [Fact]
    public void VetName_exceeding_200_chars_fails()
    {
        var cmd = new AddMedicalRecordCommand(Guid.NewGuid(), Guid.NewGuid(), "Dermatitis", "Topical cream", new string('A', 201));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VetName");
    }
}
