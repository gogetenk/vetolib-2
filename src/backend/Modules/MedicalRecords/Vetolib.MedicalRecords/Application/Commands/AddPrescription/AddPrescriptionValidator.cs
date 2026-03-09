using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal class AddPrescriptionValidator : AbstractValidator<AddPrescriptionCommand>
{
    public AddPrescriptionValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Medication).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dosage).NotEmpty().MaximumLength(500);
        RuleFor(x => x.VetLicenseNumber).NotEmpty().MaximumLength(100);
    }
}
