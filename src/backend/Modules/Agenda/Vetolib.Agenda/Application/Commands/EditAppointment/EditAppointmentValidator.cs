using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.EditAppointment;

internal class EditAppointmentValidator : AbstractValidator<EditAppointmentCommand>
{
    public EditAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("AppointmentId est requis");

        When(x => x.VeterinarianId.HasValue, () =>
        {
            RuleFor(x => x.VeterinarianId!.Value)
                .NotEmpty()
                .WithMessage("VeterinarianId ne peut pas etre un Guid vide");
        });

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value)
                .GreaterThan(0)
                .LessThanOrEqualTo(480)
                .WithMessage("La duree doit etre entre 1 et 480 minutes");
        });
    }
}
