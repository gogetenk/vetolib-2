using FluentValidation;

namespace Vetolib.Breeding.Application.Commands.RecordDelivery;

internal class RecordDeliveryValidator : AbstractValidator<RecordDeliveryCommand>
{
    public RecordDeliveryValidator()
    {
        RuleFor(x => x.PregnancyId).NotEmpty();
        RuleFor(x => x.DeliveryDate).NotEmpty();
        RuleFor(x => x.Outcome).IsInEnum();
        RuleFor(x => x.OffspringCount).GreaterThanOrEqualTo(0);
    }
}
