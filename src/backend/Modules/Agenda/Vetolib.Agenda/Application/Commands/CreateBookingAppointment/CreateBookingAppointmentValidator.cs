using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CreateBookingAppointment;

internal class CreateBookingAppointmentValidator : AbstractValidator<CreateBookingAppointmentCommand>
{
    public CreateBookingAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.VeterinarianId).NotEmpty();
        RuleFor(x => x.VeterinarianName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.AnimalName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.DurationMinutes).GreaterThan(0)
            .WithMessage("Duration must be greater than 0 minutes");
    }
}
