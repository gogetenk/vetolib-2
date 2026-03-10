using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Queries.ListStockItems;

internal record ListStockItemsQuery(
    string? Category = null,
    bool LowStock = false,
    bool ExpiringSoon = false
) : IRequest<Result<List<StockItemDto>>>;
