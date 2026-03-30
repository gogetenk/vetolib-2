using FluentAssertions;
using Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;
using Vetolib.Billing.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class UpdateInvoiceStatusValidatorTests
{
    private readonly UpdateInvoiceStatusValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdateInvoiceStatusCommand(Guid.NewGuid(), InvoiceStatus.Paid);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_invoice_id_fails()
    {
        var cmd = new UpdateInvoiceStatusCommand(Guid.Empty, InvoiceStatus.Paid);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "InvoiceId is required");
    }

    [Fact]
    public void Invalid_status_fails()
    {
        var cmd = new UpdateInvoiceStatusCommand(Guid.NewGuid(), (InvoiceStatus)999);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }
}
