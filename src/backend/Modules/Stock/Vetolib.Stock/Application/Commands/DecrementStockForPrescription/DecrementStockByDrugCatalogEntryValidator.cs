using FluentValidation;

namespace Vetolib.Stock.Application.Commands.DecrementStockForPrescription;

internal class DecrementStockByDrugCatalogEntryValidator
    : AbstractValidator<DecrementStockByDrugCatalogEntryCommand>
{
    public DecrementStockByDrugCatalogEntryValidator()
    {
        RuleFor(x => x.DrugCatalogEntryId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PrescriptionId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
