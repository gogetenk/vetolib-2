using FluentValidation;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;

internal class UpdateInvoiceStatusValidator : AbstractValidator<UpdateInvoiceStatusCommand>
{
    public UpdateInvoiceStatusValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<InvoiceStatus>())}");
    }
}
