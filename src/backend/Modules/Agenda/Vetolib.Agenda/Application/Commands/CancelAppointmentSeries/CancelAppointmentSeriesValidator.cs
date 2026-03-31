using FluentValidation;

namespace Vetolib.Agenda.Application.Commands.CancelAppointmentSeries;

internal class CancelAppointmentSeriesValidator : AbstractValidator<CancelAppointmentSeriesCommand>
{
    public CancelAppointmentSeriesValidator()
    {
        RuleFor(x => x.SeriesId)
            .NotEmpty().WithMessage("SeriesId is required.");
    }
}
