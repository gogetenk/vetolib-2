using FluentValidation;

namespace Vetolib.Billing.Application.Commands.CreateInvoice;

internal class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.ItemDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ItemUnitPrice).GreaterThan(0);
    }
}
