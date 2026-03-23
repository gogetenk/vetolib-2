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
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.InvoiceTypeCode).NotEmpty();

        // FR-specific mandatory fields
        When(x => x.CountryCode == "FR", () =>
        {
            RuleFor(x => x.SellerSiren)
                .NotEmpty().WithMessage("SellerSiren is required for French invoices")
                .Matches(@"^\d{9}$").WithMessage("SellerSiren must be a 9-digit number");

            RuleFor(x => x.SellerVatNumber)
                .NotEmpty().WithMessage("SellerVatNumber is required for French invoices");

            RuleFor(x => x.OperationType)
                .NotNull().WithMessage("OperationType is required for French invoices");
        });
    }
}
