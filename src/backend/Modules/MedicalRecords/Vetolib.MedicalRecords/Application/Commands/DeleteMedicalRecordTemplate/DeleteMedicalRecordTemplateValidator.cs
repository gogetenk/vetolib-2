using FluentValidation;

namespace Vetolib.MedicalRecords.Application.Commands.DeleteMedicalRecordTemplate;

internal class DeleteMedicalRecordTemplateValidator : AbstractValidator<DeleteMedicalRecordTemplateCommand>
{
    public DeleteMedicalRecordTemplateValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}
