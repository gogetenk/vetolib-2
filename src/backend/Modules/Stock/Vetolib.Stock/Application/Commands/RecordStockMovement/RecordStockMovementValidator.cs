using FluentValidation;

namespace Vetolib.Stock.Application.Commands.RecordStockMovement;

internal class RecordStockMovementValidator : AbstractValidator<RecordStockMovementCommand>
{
    private static readonly string[] ValidTypes = ["IN", "OUT", "ADJUSTMENT"];

    public RecordStockMovementValidator()
    {
        RuleFor(x => x.StockItemId).NotEmpty();
        RuleFor(x => x.MovementType)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t.ToUpperInvariant()))
            .WithMessage("MovementType must be IN, OUT, or ADJUSTMENT");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Movement quantity must be positive");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required");
    }
}
