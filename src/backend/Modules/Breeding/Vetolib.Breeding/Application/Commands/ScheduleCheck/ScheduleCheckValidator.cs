using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.ScheduleCheck;

internal class ScheduleCheckValidator : AbstractValidator<ScheduleCheckCommand>
{
    public ScheduleCheckValidator()
    {
        RuleFor(x => x.PregnancyId).NotEmpty();
        RuleFor(x => x.ScheduledDate).NotEmpty();
        RuleFor(x => x.CheckType).IsInEnum();
    }
}
