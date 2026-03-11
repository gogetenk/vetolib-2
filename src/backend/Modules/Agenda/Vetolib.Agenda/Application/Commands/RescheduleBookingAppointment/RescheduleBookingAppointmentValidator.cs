using FluentValidation;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.RescheduleBookingAppointment;

internal class RescheduleBookingAppointmentValidator : AbstractValidator<RescheduleBookingAppointmentCommand>
{
    public RescheduleBookingAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("AppointmentId is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");

        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage("ClinicId is required.");

        RuleFor(x => x.NewDurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.");
    }
}
