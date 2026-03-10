using Ardalis.Result;
using MediatR;

namespace Vetolib.Stock.Application.Commands.DecrementStockForPrescription;

internal record DecrementStockByDrugCatalogEntryCommand(
    Guid DrugCatalogEntryId,
    int Quantity,
    Guid PrescriptionId,
    Guid ClinicId) : IRequest<Result>;
