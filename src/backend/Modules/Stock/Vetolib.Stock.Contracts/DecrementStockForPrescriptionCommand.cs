using Ardalis.Result;
using MediatR;

namespace Vetolib.Stock.Contracts;

public record DecrementStockForPrescriptionCommand(
    Guid StockItemId,
    int Quantity,
    Guid PrescriptionId,
    Guid ClinicId) : IRequest<Result>;
