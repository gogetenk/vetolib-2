using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Commands.CreateStockItem;

internal record CreateStockItemCommand(
    string Name,
    string Category,
    int Quantity,
    string Unit,
    int MinThreshold,
    DateTime? ExpiryDate
) : IRequest<Result<StockItemDto>>;
