using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetInvoiceById;

internal class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;

    public GetInvoiceByIdHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
