using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Commands.UpdateStockItem;

internal record UpdateStockItemCommand(
    Guid StockItemId,
    string? Name,
    int? MinThreshold
) : IRequest<Result<StockItemDto>>;
