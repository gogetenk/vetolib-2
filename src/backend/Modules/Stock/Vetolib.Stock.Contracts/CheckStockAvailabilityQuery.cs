using Ardalis.Result;
using MediatR;

namespace Vetolib.Stock.Contracts;

public record CheckStockAvailabilityQuery(
    Guid DrugCatalogEntryId,
    Guid ClinicId) : IRequest<Result<StockAvailabilityResult>>;

public record StockAvailabilityResult(
    bool Available,
    int Quantity,
    string Unit,
    bool IsLowStock,
    bool IsExpiringSoon,
    List<StockAlternativeDto> Alternatives,
    Guid? StockItemId = null);

public record StockAlternativeDto(
    Guid StockItemId,
    string Name,
    Guid DrugCatalogEntryId,
    int Quantity,
    string Unit);
