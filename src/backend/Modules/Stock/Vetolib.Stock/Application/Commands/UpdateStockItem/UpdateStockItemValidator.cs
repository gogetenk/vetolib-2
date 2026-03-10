using FluentValidation;

namespace Vetolib.Stock.Application.Commands.UpdateStockItem;

internal class UpdateStockItemValidator : AbstractValidator<UpdateStockItemCommand>
{
    public UpdateStockItemValidator()
    {
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty when provided");
        });

        When(x => x.MinThreshold is not null, () =>
        {
            RuleFor(x => x.MinThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("MinThreshold must be >= 0");
        });
    }
}
