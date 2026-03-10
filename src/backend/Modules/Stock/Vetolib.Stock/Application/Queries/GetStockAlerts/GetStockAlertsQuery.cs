using Ardalis.Result;
using MediatR;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Application.Queries.GetStockAlerts;

internal record GetStockAlertsQuery : IRequest<Result<StockAlertsDto>>;
