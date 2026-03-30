using Ardalis.Result;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class MedicalRecordTemplateDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var result = MedicalRecordTemplate.Create(
            ClinicId, "Routine Checkup", TemplateCategory.Checkup,
            "General exam", "No treatment", "All normal",
            species: null, isSystemTemplate: false, sortOrder: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Routine Checkup");
        result.Value.Category.Should().Be(TemplateCategory.Checkup);
        result.Value.IsSystemTemplate.Should().BeFalse();
        result.Value.Species.Should().BeNull();
    }

    [Fact]
    public void Create_WithSpecies_Succeeds()
    {
        var result = MedicalRecordTemplate.Create(
            ClinicId, "Dog Vaccination", TemplateCategory.Vaccination,
            "Rabies vaccine", "Subcutaneous injection", "Observe 15 min",
            species: Species.Dog, isSystemTemplate: false, sortOrder: 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Species.Should().Be(Species.Dog);
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = MedicalRecordTemplate.Create(
            Guid.Empty, "Test", TemplateCategory.General,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: false, sortOrder: 0);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsInvalid()
    {
        var result = MedicalRecordTemplate.Create(
            ClinicId, "", TemplateCategory.General,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: false, sortOrder: 0);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ReturnsInvalid()
    {
        var longName = new string('A', 201);

        var result = MedicalRecordTemplate.Create(
            ClinicId, longName, TemplateCategory.General,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: false, sortOrder: 0);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = MedicalRecordTemplate.Create(
            ClinicId, "  Routine Checkup  ", TemplateCategory.Checkup,
            "  General exam  ", "  No treatment  ", "  All normal  ",
            species: null, isSystemTemplate: false, sortOrder: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Routine Checkup");
        result.Value.DiagnosisTemplate.Should().Be("General exam");
        result.Value.TreatmentTemplate.Should().Be("No treatment");
        result.Value.NotesTemplate.Should().Be("All normal");
    }

    [Fact]
    public void Update_CustomTemplate_Succeeds()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "Old Name", TemplateCategory.General,
            "old diag", "old treat", "old notes",
            species: null, isSystemTemplate: false, sortOrder: 1).Value;

        var result = template.Update(
            "New Name", TemplateCategory.Surgery,
            "new diag", "new treat", "new notes",
            Species.Cat, 5);

        result.IsSuccess.Should().BeTrue();
        template.Name.Should().Be("New Name");
        template.Category.Should().Be(TemplateCategory.Surgery);
        template.Species.Should().Be(Species.Cat);
        template.SortOrder.Should().Be(5);
    }

    [Fact]
    public void Update_SystemTemplate_ReturnsError()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "System Template", TemplateCategory.Checkup,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: true, sortOrder: 1).Value;

        var result = template.Update(
            "Modified Name", TemplateCategory.Surgery,
            "new diag", "new treat", "new notes",
            null, 2);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SYSTEM_TEMPLATE_READONLY"));
    }

    [Fact]
    public void Delete_CustomTemplate_Succeeds()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "Custom", TemplateCategory.General,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: false, sortOrder: 0).Value;

        var result = template.Delete();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Delete_SystemTemplate_ReturnsError()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "System", TemplateCategory.Checkup,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: true, sortOrder: 0).Value;

        var result = template.Delete();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SYSTEM_TEMPLATE_READONLY"));
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "Vaccination Visit", TemplateCategory.Vaccination,
            "Vaccine given", "Observed 15 min", "No reaction",
            Species.Dog, isSystemTemplate: true, sortOrder: 3).Value;

        var dto = template.ToDto();

        dto.Name.Should().Be("Vaccination Visit");
        dto.Category.Should().Be(TemplateCategory.Vaccination);
        dto.DiagnosisTemplate.Should().Be("Vaccine given");
        dto.TreatmentTemplate.Should().Be("Observed 15 min");
        dto.NotesTemplate.Should().Be("No reaction");
        dto.Species.Should().Be(Species.Dog);
        dto.IsSystemTemplate.Should().BeTrue();
        dto.SortOrder.Should().Be(3);
    }

    [Fact]
    public void SeedData_BuildsExpectedTemplates()
    {
        var clinicId = Guid.NewGuid();
        var templates = Vetolib.MedicalRecords.Infrastructure.MedicalRecordTemplateSeedData.BuildSystemTemplates(clinicId);

        templates.Should().HaveCount(5);
        templates.Should().AllSatisfy(t =>
        {
            t.ClinicId.Should().Be(clinicId);
            t.IsSystemTemplate.Should().BeTrue();
        });

        templates.Select(t => t.Name).Should().Contain("Routine Checkup");
        templates.Select(t => t.Name).Should().Contain("Vaccination Visit");
        templates.Select(t => t.Name).Should().Contain("Dental Cleaning");
        templates.Select(t => t.Name).Should().Contain("Wound Treatment");
        templates.Select(t => t.Name).Should().Contain("Post-Surgery Follow-up");
    }
}
