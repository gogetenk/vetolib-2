using FluentValidation;

namespace Vetolib.Billing.Application.Commands.SubmitToEInvoicing;

internal class SubmitToEInvoicingValidator : AbstractValidator<SubmitToEInvoicingCommand>
{
    public SubmitToEInvoicingValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required.");
    }
}
