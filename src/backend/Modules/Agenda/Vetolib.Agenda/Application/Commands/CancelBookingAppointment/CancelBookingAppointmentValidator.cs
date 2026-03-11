using FluentValidation;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CancelBookingAppointment;

internal class CancelBookingAppointmentValidator : AbstractValidator<CancelBookingAppointmentCommand>
{
    public CancelBookingAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("AppointmentId is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");

        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage("ClinicId is required.");
    }
}
