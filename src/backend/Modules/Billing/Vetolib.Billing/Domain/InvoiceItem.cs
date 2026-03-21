using Ardalis.Result;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Domain;

internal class InvoiceItem : BaseEntity
{
    private const decimal TaxRate = 0.05m; // UAE VAT 5%

    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal UnitPriceExclTax { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalInclTax { get; private set; }

    private InvoiceItem() { } // EF Core

    public static Result<InvoiceItem> Create(Guid invoiceId, string description, decimal unitPrice)
    {
        var errors = new List<ValidationError>();

        if (invoiceId == Guid.Empty)
            errors.Add(new ValidationError(nameof(invoiceId), "InvoiceId is required"));

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(new ValidationError(nameof(description), "Description is required"));

        if (unitPrice <= 0)
            errors.Add(new ValidationError(nameof(unitPrice), "Unit price must be positive"));

        if (errors.Count > 0)
            return Result<InvoiceItem>.Invalid(errors);

        var taxAmount = Math.Round(unitPrice * TaxRate, 2);

        var item = new InvoiceItem
        {
            InvoiceId = invoiceId,
            Description = description,
            UnitPriceExclTax = unitPrice,
            TaxAmount = taxAmount,
            TotalInclTax = unitPrice + taxAmount
        };

        return Result<InvoiceItem>.Success(item);
    }

    public InvoiceItemDto ToDto() => new(
        Id,
        Description,
        1,
        UnitPriceExclTax,
        TotalInclTax);
}
