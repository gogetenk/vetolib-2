using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts.EInvoicing;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetEInvoicingStatus;

internal class GetEInvoicingStatusHandler : IRequestHandler<GetEInvoicingStatusQuery, Result<EInvoiceStatus>>
{
    private readonly BillingDbContext _context;
    private readonly IEInvoicingGateway _gateway;

    public GetEInvoicingStatusHandler(BillingDbContext context, IEInvoicingGateway gateway)
    {
        _context = context;
        _gateway = gateway;
    }

    public async Task<Result<EInvoiceStatus>> Handle(GetEInvoicingStatusQuery query, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
            return Result<EInvoiceStatus>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        if (invoice.PlatformInvoiceId is null)
            return Result<EInvoiceStatus>.Error("EINVOICING_NOT_SUBMITTED:Invoice has not been submitted to e-invoicing platform");

        var statusResult = await _gateway.GetStatusAsync(invoice.PlatformInvoiceId, ct);
        if (!statusResult.IsSuccess)
            return statusResult;

        // Update the stored status if it changed
        if (statusResult.Value.Status != invoice.EInvoicingStatus)
        {
            // Re-fetch with tracking to update
            var trackedInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

            if (trackedInvoice is not null)
            {
                trackedInvoice.UpdateEInvoicingStatus(statusResult.Value.Status, statusResult.Value.RejectionReason);
                await _context.SaveChangesAsync(ct);
            }
        }

        return statusResult;
    }
}
