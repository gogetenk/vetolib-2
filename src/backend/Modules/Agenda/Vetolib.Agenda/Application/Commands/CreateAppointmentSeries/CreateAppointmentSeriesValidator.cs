using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CreateAppointmentSeries;

internal class CreateAppointmentSeriesValidator : AbstractValidator<CreateAppointmentSeriesCommand>
{
    public CreateAppointmentSeriesValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VeterinarianId).NotEmpty();
        RuleFor(x => x.VeterinarianName).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.AnimalName).NotEmpty();
        RuleFor(x => x.OwnerName).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0)
            .WithMessage("Duration must be greater than 0");
        RuleFor(x => x.Count).InclusiveBetween(2, 52)
            .WithMessage("Count must be between 2 and 52");
        RuleFor(x => x.Frequency).IsInEnum()
            .WithMessage("Frequency must be a valid recurrence frequency");
    }
}
