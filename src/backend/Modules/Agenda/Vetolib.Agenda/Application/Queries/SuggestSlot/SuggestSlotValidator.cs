using FluentValidation;

namespace Vetolib.Agenda.Application.Queries.SuggestSlot;

internal class SuggestSlotValidator : AbstractValidator<SuggestSlotQuery>
{
    public SuggestSlotValidator()
    {
        RuleFor(x => x.ConsultationType).NotEmpty()
            .WithMessage("Consultation type is required");

        RuleFor(x => x.PreferredDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Preferred date cannot be in the past");

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value).GreaterThan(0)
                .WithMessage("Duration must be greater than 0");
        });
    }
}
