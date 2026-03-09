using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CreateAppointment;

internal class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VeterinarianId).NotEmpty();
        RuleFor(x => x.VeterinarianName).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.AnimalName).NotEmpty();
        RuleFor(x => x.OwnerName).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0)
            .WithMessage("La duree doit etre superieure a 0");
    }
}
