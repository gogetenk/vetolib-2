namespace Vetolib.Billing.Contracts;

public record InvoiceItemDto(
    Guid Id,
    string Description,
    decimal UnitPriceExclTax,
    decimal TaxAmount,
    decimal TotalInclTax);
