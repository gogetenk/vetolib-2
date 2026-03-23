namespace Vetolib.Billing.Contracts;

public record CreateInvoiceRequest(
    Guid AnimalId,
    string ItemDescription,
    decimal ItemUnitPrice,
    string CountryCode = "AE",
    TaxCategory ItemTaxCategory = TaxCategory.Standard);
