using FluentValidation;

namespace Vetolib.AI.Application.Commands.ConvertAlertToAppointment;

internal class ConvertAlertToAppointmentValidator : AbstractValidator<ConvertAlertToAppointmentCommand>
{
    public ConvertAlertToAppointmentValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("AlertId is required.");
    }
}
