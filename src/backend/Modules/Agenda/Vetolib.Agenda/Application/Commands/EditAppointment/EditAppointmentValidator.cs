using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.EditAppointment;

internal class EditAppointmentValidator : AbstractValidator<EditAppointmentCommand>
{
    public EditAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("AppointmentId is required");

        When(x => x.VeterinarianId.HasValue, () =>
        {
            RuleFor(x => x.VeterinarianId!.Value)
                .NotEmpty()
                .WithMessage("VeterinarianId cannot be an empty Guid");
        });

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value)
                .GreaterThan(0)
                .LessThanOrEqualTo(480)
                .WithMessage("Duration must be between 1 and 480 minutes");
        });
    }
}
