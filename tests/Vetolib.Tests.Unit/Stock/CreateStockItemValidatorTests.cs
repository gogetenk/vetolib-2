using FluentAssertions;
using Vetolib.Stock.Application.Commands.CreateStockItem;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class CreateStockItemValidatorTests
{
    private readonly CreateStockItemValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin 250mg", "Antibiotics", 100, "tablets", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var cmd = new CreateStockItemCommand("", "Antibiotics", 100, "tablets", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public void Empty_category_fails()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin", "", 100, "tablets", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Category is required");
    }

    [Fact]
    public void Negative_quantity_fails()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin", "Antibiotics", -1, "tablets", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity cannot be negative");
    }

    [Fact]
    public void Empty_unit_fails()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin", "Antibiotics", 100, "", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Unit is required");
    }

    [Fact]
    public void Negative_min_threshold_fails()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin", "Antibiotics", 100, "tablets", -5, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "MinThreshold cannot be negative");
    }

    [Fact]
    public void Zero_quantity_passes()
    {
        var cmd = new CreateStockItemCommand("Amoxicillin", "Antibiotics", 0, "tablets", 10, null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
