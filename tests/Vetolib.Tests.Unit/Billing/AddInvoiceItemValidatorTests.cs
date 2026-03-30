using FluentAssertions;
using Vetolib.Billing.Application.Commands.AddInvoiceItem;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class AddInvoiceItemValidatorTests
{
    private readonly AddInvoiceItemValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new AddInvoiceItemCommand(Guid.NewGuid(), "Consultation fee", 150m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_invoice_id_fails()
    {
        var cmd = new AddInvoiceItemCommand(Guid.Empty, "Consultation fee", 150m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InvoiceId");
    }

    [Fact]
    public void Empty_description_fails()
    {
        var cmd = new AddInvoiceItemCommand(Guid.NewGuid(), "", 150m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Description_exceeding_500_chars_fails()
    {
        var cmd = new AddInvoiceItemCommand(Guid.NewGuid(), new string('A', 501), 150m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Zero_unit_price_fails()
    {
        var cmd = new AddInvoiceItemCommand(Guid.NewGuid(), "Consultation fee", 0m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitPrice");
    }

    [Fact]
    public void Negative_unit_price_fails()
    {
        var cmd = new AddInvoiceItemCommand(Guid.NewGuid(), "Consultation fee", -10m);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitPrice");
    }
}
