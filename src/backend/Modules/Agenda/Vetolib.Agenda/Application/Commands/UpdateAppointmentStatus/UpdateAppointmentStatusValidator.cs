using FluentValidation;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;

internal class UpdateAppointmentStatusValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("AppointmentId est requis");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage($"Le statut doit etre l'une des valeurs : {string.Join(", ", Enum.GetNames<AppointmentStatus>())}");
    }
}
