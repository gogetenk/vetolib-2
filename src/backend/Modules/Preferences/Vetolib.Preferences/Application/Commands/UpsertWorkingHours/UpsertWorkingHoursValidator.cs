using FluentValidation;

namespace Vetolib.Preferences.Application.Commands.UpsertWorkingHours;

internal class UpsertWorkingHoursValidator : AbstractValidator<UpsertWorkingHoursCommand>
{
    public UpsertWorkingHoursValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty().WithMessage("ClinicId is required");
        RuleFor(x => x.Days).NotEmpty().WithMessage("At least one day must be provided");
        RuleFor(x => x.Days)
            .Must(days => days.Select(d => d.DayOfWeek).Distinct().Count() == days.Count)
            .WithMessage("Duplicate DayOfWeek entries are not allowed");
        RuleForEach(x => x.Days).SetValidator(new WorkingHoursItemValidator());
    }
}

internal class WorkingHoursItemValidator : AbstractValidator<WorkingHoursItemRequest>
{
    public WorkingHoursItemValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("DayOfWeek must be a valid day (0=Sunday to 6=Saturday)");
    }
}
