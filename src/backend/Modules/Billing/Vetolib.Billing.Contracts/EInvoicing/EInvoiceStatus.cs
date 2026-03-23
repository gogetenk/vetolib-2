namespace Vetolib.Billing.Contracts.EInvoicing;

public record EInvoiceStatus(
    string PlatformInvoiceId,
    EInvoicingPlatformStatus Status,
    string? RejectionReason);
