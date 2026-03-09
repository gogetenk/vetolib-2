namespace Vetolib.Billing.Contracts;

public record CreateInvoiceRequest(
    Guid AnimalId,
    string ItemDescription,
    decimal ItemUnitPrice);
