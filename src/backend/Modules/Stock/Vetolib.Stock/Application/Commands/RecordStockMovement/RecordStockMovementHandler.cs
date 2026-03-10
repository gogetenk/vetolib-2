using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Commands.RecordStockMovement;

internal class RecordStockMovementHandler : IRequestHandler<RecordStockMovementCommand, Result<StockItemDto>>
{
    private readonly StockDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public RecordStockMovementHandler(StockDbContext context, IUserContext userContext, IPublisher publisher)
    {
        _context = context;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<StockItemDto>> Handle(RecordStockMovementCommand cmd, CancellationToken ct)
    {
        var item = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == cmd.StockItemId, ct);
        if (item is null)
            return Result<StockItemDto>.NotFound($"Stock item '{cmd.StockItemId}' not found.");

        if (!Enum.TryParse<StockMovementType>(cmd.MovementType, ignoreCase: true, out var movementType))
            return Result<StockItemDto>.Error($"Invalid movement type: {cmd.MovementType}");

        var moveResult = item.ApplyMovement(movementType, cmd.Quantity);
        if (!moveResult.IsSuccess)
            return moveResult.Map(_ => (StockItemDto)null!);

        var movementCreateResult = StockMovement.Create(
            item.ClinicId,
            item.Id,
            movementType,
            cmd.Quantity,
            cmd.Reason,
            _userContext.UserEmail ?? "system");

        if (!movementCreateResult.IsSuccess)
            return movementCreateResult.Map(_ => (StockItemDto)null!);

        _context.StockMovements.Add(movementCreateResult.Value);
        await _context.SaveChangesAsync(ct);

        if (movementType == StockMovementType.Out && item.IsLowStock)
        {
            await _publisher.Publish(new StockLowEvent(
                item.ClinicId,
                item.Id,
                item.Name,
                item.Quantity,
                item.MinThreshold), ct);
        }

        return Result<StockItemDto>.Success(item.ToDto());
    }
}
