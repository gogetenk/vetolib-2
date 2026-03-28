using Ardalis.Result;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Domain;

internal class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPriceExclTax { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalInclTax { get; private set; }
    public TaxCategory TaxCategory { get; private set; }
    public decimal TaxRate { get; private set; }

    private InvoiceItem() { } // EF Core

    public static Result<InvoiceItem> Create(
        Guid invoiceId,
        string description,
        decimal unitPrice,
        decimal taxRate,
        TaxCategory taxCategory = TaxCategory.Standard,
        int quantity = 1)
    {
        var errors = new List<ValidationError>();

        if (invoiceId == Guid.Empty)
            errors.Add(new ValidationError(nameof(invoiceId), "InvoiceId is required"));

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(new ValidationError(nameof(description), "Description is required"));

        if (unitPrice <= 0)
            errors.Add(new ValidationError(nameof(unitPrice), "Unit price must be positive"));

        if (taxRate < 0)
            errors.Add(new ValidationError(nameof(taxRate), "Tax rate cannot be negative"));

        if (quantity <= 0)
            errors.Add(new ValidationError(nameof(quantity), "Quantity must be positive"));

        if (errors.Count > 0)
            return Result<InvoiceItem>.Invalid(errors);

        var taxAmount = Math.Round(unitPrice * quantity * taxRate, 2);

        var item = new InvoiceItem
        {
            InvoiceId = invoiceId,
            Description = description,
            Quantity = quantity,
            UnitPriceExclTax = unitPrice,
            TaxRate = taxRate,
            TaxCategory = taxCategory,
            TaxAmount = taxAmount,
            TotalInclTax = unitPrice * quantity + taxAmount
        };

        return Result<InvoiceItem>.Success(item);
    }

    public InvoiceItemDto ToDto() => new(
        Id,
        Description,
        Quantity,
        UnitPriceExclTax,
        TotalInclTax,
        TaxCategory,
        TaxRate);
}
