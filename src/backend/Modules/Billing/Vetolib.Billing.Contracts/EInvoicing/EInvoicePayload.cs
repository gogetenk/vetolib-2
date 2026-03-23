namespace Vetolib.Billing.Contracts.EInvoicing;

public record EInvoicePayload(
    string InvoiceNumber,
    byte[] FacturXPdf,
    byte[] CiiXml,
    string SellerSiren,
    string SellerVatNumber,
    string BuyerName,
    string? BuyerSiren);
