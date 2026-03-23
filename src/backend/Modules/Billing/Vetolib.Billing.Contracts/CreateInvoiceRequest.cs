namespace Vetolib.Billing.Contracts;

public record CreateInvoiceRequest(
    Guid AnimalId,
    string ItemDescription,
    decimal ItemUnitPrice,
    TaxCategory ItemTaxCategory = TaxCategory.Standard,
    string BuyerName = "",
    string CountryCode = "AE",
    string InvoiceTypeCode = "380",
    string? SellerSiren = null,
    string? SellerVatNumber = null,
    string? BuyerSiren = null,
    string? BuyerVatNumber = null,
    string? BuyerAddress = null,
    OperationType? OperationType = null,
    string? PaymentTerms = null,
    string? PurchaseOrderReference = null);
