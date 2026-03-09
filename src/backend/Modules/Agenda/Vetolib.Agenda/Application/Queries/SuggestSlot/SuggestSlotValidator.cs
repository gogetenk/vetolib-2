using FluentValidation;

namespace Vetolib.Agenda.Application.Queries.SuggestSlot;

internal class SuggestSlotValidator : AbstractValidator<SuggestSlotQuery>
{
    public SuggestSlotValidator()
    {
        RuleFor(x => x.ConsultationType).NotEmpty()
            .WithMessage("Le type de consultation est obligatoire");

        RuleFor(x => x.PreferredDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("La date preferee ne peut pas etre dans le passe");

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value).GreaterThan(0)
                .WithMessage("La duree doit etre superieure a 0");
        });
    }
}
