using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.AddPrescription;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddPrescriptionValidatorTests
{
    private readonly AddPrescriptionValidator _validator = new();

    private static AddPrescriptionCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "Amoxicillin 250mg", "2x daily", "VET-12345", Guid.NewGuid(), "Vet");

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_medical_record_id_fails()
    {
        var cmd = ValidCommand() with { MedicalRecordId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MedicalRecordId");
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
    public void Empty_patient_id_fails()
    {
        var cmd = ValidCommand() with { PatientId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Empty_medication_fails()
    {
        var cmd = ValidCommand() with { Medication = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Medication");
    }

    [Fact]
    public void Medication_exceeding_500_chars_fails()
    {
        var cmd = ValidCommand() with { Medication = new string('A', 501) };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Medication");
    }

    [Fact]
    public void Empty_dosage_fails()
    {
        var cmd = ValidCommand() with { Dosage = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dosage");
    }

    [Fact]
    public void Dosage_exceeding_500_chars_fails()
    {
        var cmd = ValidCommand() with { Dosage = new string('A', 501) };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dosage");
    }

    [Fact]
    public void Empty_vet_license_number_fails()
    {
        var cmd = ValidCommand() with { VetLicenseNumber = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VetLicenseNumber");
    }

    [Fact]
    public void VetLicenseNumber_exceeding_100_chars_fails()
    {
        var cmd = ValidCommand() with { VetLicenseNumber = new string('A', 101) };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VetLicenseNumber");
    }

    [Fact]
    public void Empty_vet_id_fails()
    {
        var cmd = ValidCommand() with { VetId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VetId");
    }

    [Fact]
    public void Short_override_justification_fails()
    {
        var cmd = ValidCommand() with { OverrideJustification = "short" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Override justification must be at least 10 characters");
    }

    [Fact]
    public void Valid_override_justification_passes()
    {
        var cmd = ValidCommand() with { OverrideJustification = "Patient requires higher dose due to weight" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Null_override_justification_passes()
    {
        var cmd = ValidCommand() with { OverrideJustification = null };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
