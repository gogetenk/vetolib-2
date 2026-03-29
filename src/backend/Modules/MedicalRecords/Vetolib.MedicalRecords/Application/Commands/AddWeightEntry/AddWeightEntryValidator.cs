using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.AddWeightEntry;

internal class AddWeightEntryValidator : AbstractValidator<AddWeightEntryCommand>
{
    public AddWeightEntryValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.WeightKg).GreaterThan(0).WithMessage("Weight must be greater than zero");
        RuleFor(x => x.WeightKg).LessThanOrEqualTo(10000).WithMessage("Weight exceeds maximum allowed value");
        RuleFor(x => x.RecordedBy).NotEmpty().WithMessage("RecordedBy is required");
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
