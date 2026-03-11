using FluentValidation.TestHelper;
using Vetolib.Stock.Application.Commands.DecrementStockForPrescription;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class DecrementStockByDrugCatalogEntryValidatorTests
{
    private readonly DecrementStockByDrugCatalogEntryValidator _validator = new();

    private static DecrementStockByDrugCatalogEntryCommand ValidCommand() =>
        new(
            DrugCatalogEntryId: Guid.NewGuid(),
            Quantity: 1,
            PrescriptionId: Guid.NewGuid(),
            ClinicId: Guid.NewGuid());

    [Fact]
    public void Should_not_have_errors_when_all_fields_are_valid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_have_error_when_DrugCatalogEntryId_is_empty_guid()
    {
        var command = ValidCommand() with { DrugCatalogEntryId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DrugCatalogEntryId);
    }

    [Fact]
    public void Should_have_error_when_Quantity_is_zero()
    {
        var command = ValidCommand() with { Quantity = 0 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_have_error_when_Quantity_is_negative()
    {
        var command = ValidCommand() with { Quantity = -1 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_not_have_error_when_Quantity_is_positive()
    {
        var command = ValidCommand() with { Quantity = 5 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_have_error_when_PrescriptionId_is_empty_guid()
    {
        var command = ValidCommand() with { PrescriptionId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PrescriptionId);
    }

    [Fact]
    public void Should_have_error_when_ClinicId_is_empty_guid()
    {
        var command = ValidCommand() with { ClinicId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClinicId);
    }
}
