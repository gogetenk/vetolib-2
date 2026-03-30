using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.CreateMedicalRecordTemplate;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreateMedicalRecordTemplateValidatorTests
{
    private readonly CreateMedicalRecordTemplateValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            Guid.NewGuid(), "Template", TemplateCategory.General,
            "diag", "treat", "notes", null, 0);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyClinicId_Fails()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            Guid.Empty, "Template", TemplateCategory.General,
            "diag", "treat", "notes", null, 0);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            Guid.NewGuid(), "", TemplateCategory.General,
            "diag", "treat", "notes", null, 0);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_Fails()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            Guid.NewGuid(), new string('A', 201), TemplateCategory.General,
            "diag", "treat", "notes", null, 0);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_DiagnosisExceedsMaxLength_Fails()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            Guid.NewGuid(), "Template", TemplateCategory.General,
            new string('A', 2001), "treat", "notes", null, 0);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DiagnosisTemplate");
    }
}
