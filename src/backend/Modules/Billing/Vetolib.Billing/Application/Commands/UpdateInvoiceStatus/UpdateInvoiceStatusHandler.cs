using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;

internal class UpdateInvoiceStatusHandler : IRequestHandler<UpdateInvoiceStatusCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;
    private readonly BillingOptions _options;

    public UpdateInvoiceStatusHandler(BillingDbContext context, IOptions<BillingOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<InvoiceDto>> Handle(UpdateInvoiceStatusCommand cmd, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        var result = invoice.UpdateStatus(cmd.NewStatus, _options.DueDateDays);
        if (!result.IsSuccess)
            return Result<InvoiceDto>.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoice.ToDto(_options.TaxRate));
    }
}
