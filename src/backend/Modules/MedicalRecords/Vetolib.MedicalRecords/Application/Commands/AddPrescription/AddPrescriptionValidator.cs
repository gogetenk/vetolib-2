using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal class AddPrescriptionValidator : AbstractValidator<AddPrescriptionCommand>
{
    public AddPrescriptionValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Medication).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dosage).NotEmpty().MaximumLength(500);
        RuleFor(x => x.VetLicenseNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.VetId).NotEmpty();

        When(x => x.OverrideJustification is not null, () =>
        {
            RuleFor(x => x.OverrideJustification)
                .MinimumLength(10)
                .WithMessage("Override justification must be at least 10 characters");
        });
    }
}
