using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;

internal class AddMedicalRecordValidator : AbstractValidator<AddMedicalRecordCommand>
{
    public AddMedicalRecordValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Diagnosis).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Treatment).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.VetName).NotEmpty().MaximumLength(200);
    }
}
