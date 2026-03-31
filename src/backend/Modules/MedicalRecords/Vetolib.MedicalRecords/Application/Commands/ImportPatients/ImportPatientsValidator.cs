using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatients;

internal class ImportPatientsValidator : AbstractValidator<ImportPatientsCommand>
{
    public ImportPatientsValidator()
    {
        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage("ClinicId is required.");

        RuleFor(x => x.CsvStream)
            .NotNull().WithMessage("CSV stream is required.");
    }
}
