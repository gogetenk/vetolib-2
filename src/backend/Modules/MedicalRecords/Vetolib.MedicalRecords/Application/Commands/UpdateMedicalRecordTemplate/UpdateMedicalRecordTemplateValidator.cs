using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.UpdateMedicalRecordTemplate;

internal class UpdateMedicalRecordTemplateValidator : AbstractValidator<UpdateMedicalRecordTemplateCommand>
{
    public UpdateMedicalRecordTemplateValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.DiagnosisTemplate).MaximumLength(2000);
        RuleFor(x => x.TreatmentTemplate).MaximumLength(2000);
        RuleFor(x => x.NotesTemplate).MaximumLength(2000);
    }
}
