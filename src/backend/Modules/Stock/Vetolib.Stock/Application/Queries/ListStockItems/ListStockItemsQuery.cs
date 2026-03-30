using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Queries.ListStockItems;

internal record ListStockItemsQuery(
    string? Category = null,
    bool LowStock = false,
    bool ExpiringSoon = false,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<List<StockItemDto>>>;
