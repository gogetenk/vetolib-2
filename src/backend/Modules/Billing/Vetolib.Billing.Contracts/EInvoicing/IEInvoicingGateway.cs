using Ardalis.Result;

namespace Vetolib.Billing.Contracts.EInvoicing;

/// <summary>
/// Gateway abstraction for submitting invoices to PDP/PPF e-invoicing platforms.
/// The concrete implementation will depend on the chosen PDP provider.
/// </summary>
public interface IEInvoicingGateway
{
    Task<Result<EInvoiceSubmissionResult>> SubmitInvoiceAsync(EInvoicePayload payload, CancellationToken ct);
    Task<Result<EInvoiceStatus>> GetStatusAsync(string platformInvoiceId, CancellationToken ct);
}
