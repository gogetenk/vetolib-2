using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Commands.UpdateStockItem;

internal class UpdateStockItemHandler : IRequestHandler<UpdateStockItemCommand, Result<StockItemDto>>
{
    private readonly StockDbContext _context;

    public UpdateStockItemHandler(StockDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StockItemDto>> Handle(UpdateStockItemCommand cmd, CancellationToken ct)
    {
        var item = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == cmd.StockItemId, ct);
        if (item is null)
            return Result<StockItemDto>.NotFound($"Stock item '{cmd.StockItemId}' not found.");

        var updateResult = item.Update(cmd.Name, cmd.MinThreshold);
        if (!updateResult.IsSuccess)
            return updateResult.Map(_ => (StockItemDto)null!);

        await _context.SaveChangesAsync(ct);

        return Result<StockItemDto>.Success(item.ToDto());
    }
}
