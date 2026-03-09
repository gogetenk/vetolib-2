using FluentValidation;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;

internal class UpdateInvoiceStatusValidator : AbstractValidator<UpdateInvoiceStatusCommand>
{
    public UpdateInvoiceStatusValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId est requis");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage($"Le statut doit etre l'une des valeurs : {string.Join(", ", Enum.GetNames<InvoiceStatus>())}");
    }
}
