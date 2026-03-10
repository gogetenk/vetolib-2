using FluentValidation;

namespace Vetolib.Stock.Application.Commands.CreateStockItem;

internal class CreateStockItemValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");
        RuleFor(x => x.Unit).NotEmpty().WithMessage("Unit is required");
        RuleFor(x => x.MinThreshold).GreaterThanOrEqualTo(0).WithMessage("MinThreshold cannot be negative");
    }
}
