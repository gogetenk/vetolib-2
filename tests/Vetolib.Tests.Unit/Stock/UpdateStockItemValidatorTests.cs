using FluentAssertions;
using Vetolib.Stock.Application.Commands.UpdateStockItem;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class UpdateStockItemValidatorTests
{
    private readonly UpdateStockItemValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), "Updated Name", 20);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Null_name_passes()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), null, 20);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_when_provided_fails()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), "", 20);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name cannot be empty when provided");
    }

    [Fact]
    public void Null_min_threshold_passes()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), "Name", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Negative_min_threshold_fails()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), "Name", -1);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "MinThreshold must be >= 0");
    }

    [Fact]
    public void Zero_min_threshold_passes()
    {
        var cmd = new UpdateStockItemCommand(Guid.NewGuid(), "Name", 0);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
