using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Stock.Domain;

internal class StockMovement : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid StockItemId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;

    private StockMovement() { }

    public static Result<StockMovement> Create(
        Guid clinicId,
        Guid stockItemId,
        StockMovementType movementType,
        int quantity,
        string reason,
        string createdBy)
    {
        if (quantity <= 0)
            return Result<StockMovement>.Error("Movement quantity must be greater than zero");

        return Result<StockMovement>.Success(new StockMovement
        {
            ClinicId = clinicId,
            StockItemId = stockItemId,
            MovementType = movementType,
            Quantity = quantity,
            Reason = reason,
            CreatedBy = createdBy
        });
    }
}
