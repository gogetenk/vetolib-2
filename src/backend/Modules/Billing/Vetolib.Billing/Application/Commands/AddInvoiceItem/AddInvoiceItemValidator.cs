using FluentValidation;

namespace Vetolib.Billing.Application.Commands.AddInvoiceItem;

internal class AddInvoiceItemValidator : AbstractValidator<AddInvoiceItemCommand>
{
    public AddInvoiceItemValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
