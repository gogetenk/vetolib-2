using Ardalis.Result;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Contracts;

namespace Vetolib.Stock.Domain;

internal class StockItem : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public StockItemCategory Category { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public int MinThreshold { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private StockItem() { }

    public static Result<StockItem> Create(
        Guid clinicId,
        string name,
        string category,
        int quantity,
        string unit,
        int minThreshold,
        DateTime? expiryDate)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));
        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));
        if (quantity < 0)
            errors.Add(new ValidationError(nameof(quantity), "Quantity cannot be negative"));
        if (minThreshold < 0)
            errors.Add(new ValidationError(nameof(minThreshold), "MinThreshold cannot be negative"));
        if (string.IsNullOrWhiteSpace(unit))
            errors.Add(new ValidationError(nameof(unit), "Unit is required"));

        if (!Enum.TryParse<StockItemCategory>(category, ignoreCase: true, out var parsedCategory))
            errors.Add(new ValidationError(nameof(category), $"Category '{category}' is invalid. Valid values: Medication, Vaccine, Supply"));

        if (errors.Count > 0)
            return Result<StockItem>.Invalid(errors);

        return Result<StockItem>.Success(new StockItem
        {
            ClinicId = clinicId,
            Name = name,
            Category = parsedCategory,
            Quantity = quantity,
            Unit = unit,
            MinThreshold = minThreshold,
            ExpiryDate = expiryDate
        });
    }

    public Result ApplyMovement(StockMovementType movementType, int quantity)
    {
        if (quantity <= 0)
            return Result.Error("Movement quantity must be positive");

        if (movementType == StockMovementType.Out && quantity > Quantity)
            return Result.Error($"INSUFFICIENT_STOCK:Cannot remove {quantity} units — only {Quantity} in stock");

        Quantity = movementType switch
        {
            StockMovementType.In => Quantity + quantity,
            StockMovementType.Out => Quantity - quantity,
            StockMovementType.Adjustment => quantity,
            _ => Quantity
        };

        return Result.Success();
    }

    public Result Update(string? name, int? minThreshold)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Error("Name cannot be empty");
            Name = name;
        }

        if (minThreshold is not null)
        {
            if (minThreshold < 0)
                return Result.Error("MinThreshold cannot be negative");
            MinThreshold = minThreshold.Value;
        }

        return Result.Success();
    }

    public bool IsLowStock => Quantity < MinThreshold;
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.UtcNow.AddDays(30);

    public StockItemDto ToDto() => new(
        Id,
        ClinicId,
        Name,
        Category.ToString(),
        Quantity,
        Unit,
        MinThreshold,
        ExpiryDate,
        IsLowStock,
        IsExpiringSoon,
        CreatedAt,
        UpdatedAt
    );
}
