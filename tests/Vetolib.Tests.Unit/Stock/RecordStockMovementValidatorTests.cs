using FluentAssertions;
using Vetolib.Stock.Application.Commands.RecordStockMovement;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class RecordStockMovementValidatorTests
{
    private readonly RecordStockMovementValidator _validator = new();

    [Fact]
    public void Valid_IN_command_passes()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "IN", 10, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_OUT_command_passes()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "OUT", 5, "Used for patient");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_ADJUSTMENT_command_passes()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "ADJUSTMENT", 3, "Inventory correction");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Lowercase_movement_type_passes()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "in", 10, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_stock_item_id_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.Empty, "IN", 10, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockItemId");
    }

    [Fact]
    public void Invalid_movement_type_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "INVALID", 10, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "MovementType must be IN, OUT, or ADJUSTMENT");
    }

    [Fact]
    public void Empty_movement_type_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "", 10, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovementType");
    }

    [Fact]
    public void Zero_quantity_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "IN", 0, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Movement quantity must be positive");
    }

    [Fact]
    public void Negative_quantity_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "IN", -5, "Restocking");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Movement quantity must be positive");
    }

    [Fact]
    public void Empty_reason_fails()
    {
        var cmd = new RecordStockMovementCommand(Guid.NewGuid(), "IN", 10, "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Reason is required");
    }
}
