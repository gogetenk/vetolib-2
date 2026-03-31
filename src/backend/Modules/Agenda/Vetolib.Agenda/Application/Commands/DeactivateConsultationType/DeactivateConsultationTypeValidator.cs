using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.DeactivateConsultationType;

internal class DeactivateConsultationTypeValidator : AbstractValidator<DeactivateConsultationTypeCommand>
{
    public DeactivateConsultationTypeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Consultation type Id is required.");
    }
}
