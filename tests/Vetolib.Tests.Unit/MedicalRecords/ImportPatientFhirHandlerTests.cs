using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class ImportPatientFhirHandlerTests
{
    [Fact]
    public void ImportPatientFhirCommand_WithValidData_CreatesCommand()
    {
        var clinicId = Guid.NewGuid();
        var json = """{"resourceType": "Bundle", "type": "collection", "entry": []}""";

        var cmd = new ImportPatientFhirCommand(clinicId, json);

        cmd.ClinicId.Should().Be(clinicId);
        cmd.FhirBundleJson.Should().Be(json);
    }

    [Fact]
    public void ImportPatientFhirValidator_EmptyClinicId_Fails()
    {
        var validator = new ImportPatientFhirValidator();
        var cmd = new ImportPatientFhirCommand(Guid.Empty, """{"resourceType":"Bundle"}""");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicId");
    }

    [Fact]
    public void ImportPatientFhirValidator_EmptyJson_Fails()
    {
        var validator = new ImportPatientFhirValidator();
        var cmd = new ImportPatientFhirCommand(Guid.NewGuid(), "");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FhirBundleJson");
    }

    [Fact]
    public void ImportPatientFhirValidator_ValidCommand_Passes()
    {
        var validator = new ImportPatientFhirValidator();
        var cmd = new ImportPatientFhirCommand(Guid.NewGuid(), """{"resourceType":"Bundle"}""");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ImportPatientFhirValidator_OversizedJson_Fails()
    {
        var validator = new ImportPatientFhirValidator();
        var oversized = new string('x', 6 * 1024 * 1024); // 6 MB
        var cmd = new ImportPatientFhirCommand(Guid.NewGuid(), oversized);

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("5 MB"));
    }
}
