using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;

internal class ImportPatientFhirValidator : AbstractValidator<ImportPatientFhirCommand>
{
    private const int MaxBundleSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ImportPatientFhirValidator()
    {
        RuleFor(x => x.ClinicId)
            .NotEmpty()
            .WithMessage("ClinicId is required");

        RuleFor(x => x.FhirBundleJson)
            .NotEmpty()
            .WithMessage("FHIR Bundle JSON is required")
            .Must(json => json.Length <= MaxBundleSizeBytes)
            .WithMessage("FHIR Bundle JSON exceeds maximum size of 5 MB");
    }
}
