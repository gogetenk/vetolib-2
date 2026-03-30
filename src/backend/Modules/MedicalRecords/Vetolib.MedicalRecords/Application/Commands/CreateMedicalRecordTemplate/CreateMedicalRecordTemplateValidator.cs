using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.CreateMedicalRecordTemplate;

internal class CreateMedicalRecordTemplateValidator : AbstractValidator<CreateMedicalRecordTemplateCommand>
{
    public CreateMedicalRecordTemplateValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.DiagnosisTemplate).MaximumLength(2000);
        RuleFor(x => x.TreatmentTemplate).MaximumLength(2000);
        RuleFor(x => x.NotesTemplate).MaximumLength(2000);
    }
}
