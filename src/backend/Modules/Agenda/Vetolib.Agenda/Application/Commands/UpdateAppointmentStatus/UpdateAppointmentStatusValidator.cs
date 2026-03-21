using FluentValidation;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;

internal class UpdateAppointmentStatusValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("AppointmentId is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage($"Status must be one of the following values: {string.Join(", ", Enum.GetNames<AppointmentStatus>())}");
    }
}
