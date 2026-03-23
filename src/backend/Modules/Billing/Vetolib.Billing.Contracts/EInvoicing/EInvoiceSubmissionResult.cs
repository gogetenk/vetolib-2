namespace Vetolib.Billing.Contracts.EInvoicing;

public record EInvoiceSubmissionResult(
    string PlatformInvoiceId,
    EInvoicingPlatformStatus Status,
    DateTime SubmittedAt);
