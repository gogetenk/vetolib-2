using Ardalis.Result;
using MediatR;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Commands.CreateStockItem;

internal class CreateStockItemHandler : IRequestHandler<CreateStockItemCommand, Result<StockItemDto>>
{
    private readonly StockDbContext _context;
    private readonly IClinicContext _clinicContext;

    public CreateStockItemHandler(StockDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<StockItemDto>> Handle(CreateStockItemCommand cmd, CancellationToken ct)
    {
        var itemResult = StockItem.Create(
            _clinicContext.ClinicId,
            cmd.Name,
            cmd.Category,
            cmd.Quantity,
            cmd.Unit,
            cmd.MinThreshold,
            cmd.ExpiryDate,
            cmd.DrugCatalogEntryId);

        if (!itemResult.IsSuccess)
            return itemResult.Map(_ => (StockItemDto)null!);

        _context.StockItems.Add(itemResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<StockItemDto>.Success(itemResult.Value.ToDto());
    }
}
