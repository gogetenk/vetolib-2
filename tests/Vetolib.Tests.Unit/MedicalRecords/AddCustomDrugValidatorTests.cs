using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.AddCustomDrug;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddCustomDrugValidatorTests
{
    private readonly AddCustomDrugValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new AddCustomDrugCommand("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_inn_name_fails()
    {
        var cmd = new AddCustomDrugCommand("", "Amoxicillin 250mg", DrugCategory.Antibiotic, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "INN name is required");
    }

    [Fact]
    public void Empty_display_name_fails()
    {
        var cmd = new AddCustomDrugCommand("Amoxicillin", "", DrugCategory.Antibiotic, Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
    }

    [Fact]
    public void Empty_clinic_id_fails()
    {
        var cmd = new AddCustomDrugCommand("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic, Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "ClinicId is required");
    }
}
