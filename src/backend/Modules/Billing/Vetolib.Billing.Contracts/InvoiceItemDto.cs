namespace Vetolib.Billing.Contracts;

public record InvoiceItemDto(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPriceExclTax,
    decimal TotalInclTax,
    TaxCategory TaxCategory,
    decimal TaxRate);
