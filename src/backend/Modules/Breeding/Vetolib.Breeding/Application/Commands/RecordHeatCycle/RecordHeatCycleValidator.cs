using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.RecordHeatCycle;

internal class RecordHeatCycleValidator : AbstractValidator<RecordHeatCycleCommand>
{
    public RecordHeatCycleValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}
