using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Commands.RecordStockMovement;

internal record RecordStockMovementCommand(
    Guid StockItemId,
    string MovementType,
    int Quantity,
    string Reason
) : IRequest<Result<StockItemDto>>;
