using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;

internal class UpdateInvoiceStatusHandler : IRequestHandler<UpdateInvoiceStatusCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;

    public UpdateInvoiceStatusHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> Handle(UpdateInvoiceStatusCommand cmd, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Facture introuvable");

        var result = invoice.UpdateStatus(cmd.NewStatus);
        if (!result.IsSuccess)
            return Result<InvoiceDto>.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
