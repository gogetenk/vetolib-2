using FluentAssertions;
using Vetolib.Billing.Application.Commands.CreateInvoice;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class CreateInvoiceValidatorTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly CreateInvoiceValidator _validator = new();

    private CreateInvoiceCommand BuildCommand(
        Guid? clinicId = null,
        Guid? animalId = null,
        string description = "Consultation vétérinaire",
        decimal unitPrice = 150m)
        => new(
            ClinicId: clinicId ?? ClinicId,
            AnimalId: animalId ?? AnimalId,
            ItemDescription: description,
            ItemUnitPrice: unitPrice);

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var cmd = BuildCommand();

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyClinicId_FailsValidation()
    {
        var cmd = BuildCommand(clinicId: Guid.Empty);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ClinicId));
    }

    [Fact]
    public void Validate_EmptyAnimalId_FailsValidation()
    {
        var cmd = BuildCommand(animalId: Guid.Empty);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.AnimalId));
    }

    [Fact]
    public void Validate_EmptyDescription_FailsValidation()
    {
        var cmd = BuildCommand(description: "");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ItemDescription));
    }

    [Fact]
    public void Validate_WhitespaceDescription_FailsValidation()
    {
        var cmd = BuildCommand(description: "   ");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ItemDescription));
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        var cmd = BuildCommand(description: new string('A', 501));

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ItemDescription));
    }

    [Fact]
    public void Validate_NegativeUnitPrice_FailsValidation()
    {
        var cmd = BuildCommand(unitPrice: -10m);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ItemUnitPrice));
    }

    [Fact]
    public void Validate_ZeroUnitPrice_FailsValidation()
    {
        var cmd = BuildCommand(unitPrice: 0m);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.ItemUnitPrice));
    }

    [Fact]
    public void Validate_DescriptionAtMaxLength_PassesValidation()
    {
        var cmd = BuildCommand(description: new string('A', 500));

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MinimumPositiveUnitPrice_PassesValidation()
    {
        var cmd = BuildCommand(unitPrice: 0.01m);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
