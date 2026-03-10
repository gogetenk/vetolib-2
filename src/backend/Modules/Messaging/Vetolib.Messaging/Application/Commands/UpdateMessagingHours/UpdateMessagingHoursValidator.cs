using FluentValidation;

namespace Vetolib.Messaging.Application.Commands.UpdateMessagingHours;

internal class UpdateMessagingHoursValidator : AbstractValidator<UpdateMessagingHoursCommand>
{
    public UpdateMessagingHoursValidator()
    {
        RuleFor(x => x.Days)
            .NotEmpty().WithMessage("At least one day configuration is required");

        RuleForEach(x => x.Days).ChildRules(day =>
        {
            day.RuleFor(d => d.DayOfWeek)
                .InclusiveBetween(0, 6).WithMessage("DayOfWeek must be between 0 (Sunday) and 6 (Saturday)");

            day.When(d => !d.IsClosed, () =>
            {
                day.RuleFor(d => d.OpenTime)
                    .Must((d, openTime) => openTime < d.CloseTime)
                    .WithMessage("OpenTime must be before CloseTime when the day is not closed");
            });
        });
    }
}
