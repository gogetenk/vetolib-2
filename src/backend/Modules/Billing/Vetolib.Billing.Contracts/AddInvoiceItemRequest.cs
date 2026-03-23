namespace Vetolib.Billing.Contracts;

public record AddInvoiceItemRequest(
    string Description,
    decimal UnitPrice,
    TaxCategory TaxCategory = TaxCategory.Standard);
