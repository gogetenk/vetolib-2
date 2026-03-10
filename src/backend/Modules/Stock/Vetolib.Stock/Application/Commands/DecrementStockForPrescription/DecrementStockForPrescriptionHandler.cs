using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock.Application.Commands.DecrementStockForPrescription;

internal class DecrementStockForPrescriptionHandler : IRequestHandler<DecrementStockForPrescriptionCommand, Result>
{
    private readonly StockDbContext _context;
    private readonly IPublisher _publisher;

    public DecrementStockForPrescriptionHandler(StockDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Result> Handle(DecrementStockForPrescriptionCommand cmd, CancellationToken ct)
    {
        var item = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == cmd.StockItemId, ct);
        if (item is null)
            return Result.NotFound($"Stock item '{cmd.StockItemId}' not found.");

        var moveResult = item.ApplyMovement(StockMovementType.Out, cmd.Quantity);
        if (!moveResult.IsSuccess)
            return moveResult;

        var reason = $"Prescription #{cmd.PrescriptionId}";
        var movementResult = StockMovement.Create(
            item.ClinicId,
            item.Id,
            StockMovementType.Out,
            cmd.Quantity,
            reason,
            "system");

        if (!movementResult.IsSuccess)
            return movementResult.Map(_ => Result.Error());

        _context.StockMovements.Add(movementResult.Value);
        await _context.SaveChangesAsync(ct);

        if (item.IsLowStock)
        {
            await _publisher.Publish(new StockLowEvent(
                item.ClinicId,
                item.Id,
                item.Name,
                item.Quantity,
                item.MinThreshold), ct);
        }

        return Result.Success();
    }
}
