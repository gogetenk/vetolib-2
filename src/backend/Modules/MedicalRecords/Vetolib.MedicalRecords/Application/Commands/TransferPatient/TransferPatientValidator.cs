using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.TransferPatient;

internal class TransferPatientValidator : AbstractValidator<TransferPatientCommand>
{
    public TransferPatientValidator()
    {
        RuleFor(x => x.SourceClinicId)
            .NotEmpty().WithMessage("Source clinic ID is required");

        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required");

        RuleFor(x => x.TargetClinicId)
            .NotEmpty().WithMessage("Target clinic ID is required");

        RuleFor(x => x.TargetClinicId)
            .NotEqual(x => x.SourceClinicId)
            .WithMessage("Cannot transfer a patient to the same clinic");

        RuleFor(x => x.TransferredBy)
            .NotEmpty().WithMessage("TransferredBy is required");
    }
}
