using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.AI.Application.Commands.GenerateSoapNotes;
using Vetolib.AI.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class GenerateSoapNotesValidatorTests
{
    private readonly GenerateSoapNotesValidator _sut = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "Dog",
            Breed: "Labrador",
            PatientName: "Buddy",
            Symptoms: "Limping",
            Vitals: "Normal",
            Diagnosis: "Sprain",
            TreatmentPlan: "Rest",
            Prescriptions: new List<string> { "Meloxicam" });

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Dog", "Buddy")]
    [InlineData("Limping", "", "Buddy")]
    [InlineData("Limping", "Dog", "")]
    public void Validate_WithMissingRequiredFields_ShouldHaveErrors(
        string symptoms, string species, string patientName)
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: species,
            Breed: "Labrador",
            PatientName: patientName,
            Symptoms: symptoms,
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        var result = _sut.TestValidate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptySpecies_ShouldHaveSpeciesError()
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "",
            Breed: "Labrador",
            PatientName: "Buddy",
            Symptoms: "Limping",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Species)
            .WithErrorMessage("Species is required.");
    }

    [Fact]
    public void Validate_WithEmptySymptoms_ShouldHaveSymptomsError()
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "Cat",
            Breed: "Persian",
            PatientName: "Whiskers",
            Symptoms: "",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Symptoms)
            .WithErrorMessage("Symptoms are required.");
    }

    [Fact]
    public void Validate_WithEmptyPatientName_ShouldHavePatientNameError()
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "Dog",
            Breed: "Labrador",
            PatientName: "",
            Symptoms: "Limping",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PatientName)
            .WithErrorMessage("Patient name is required.");
    }

    [Theory]
    [InlineData(SoapLanguage.En)]
    [InlineData(SoapLanguage.Ar)]
    [InlineData(SoapLanguage.Both)]
    public void Validate_WithValidLanguage_ShouldHaveNoLanguageError(SoapLanguage language)
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "Dog",
            Breed: "Labrador",
            PatientName: "Buddy",
            Symptoms: "Limping",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>(),
            Language: language);

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_WithInvalidLanguage_ShouldHaveLanguageError()
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: "Dog",
            Breed: "Labrador",
            PatientName: "Buddy",
            Symptoms: "Limping",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>(),
            Language: (SoapLanguage)99);

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Language)
            .WithErrorMessage("Language must be 'En', 'Ar', or 'Both'.");
    }
}
