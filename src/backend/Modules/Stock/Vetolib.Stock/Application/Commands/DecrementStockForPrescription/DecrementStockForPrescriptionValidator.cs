using FluentValidation;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Commands.DecrementStockForPrescription;

internal class DecrementStockForPrescriptionValidator : AbstractValidator<DecrementStockForPrescriptionCommand>
{
    public DecrementStockForPrescriptionValidator()
    {
        RuleFor(x => x.StockItemId)
            .NotEmpty().WithMessage("StockItemId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.PrescriptionId)
            .NotEmpty().WithMessage("PrescriptionId is required.");

        RuleFor(x => x.ClinicId)
            .NotEmpty().WithMessage("ClinicId is required.");
    }
}
