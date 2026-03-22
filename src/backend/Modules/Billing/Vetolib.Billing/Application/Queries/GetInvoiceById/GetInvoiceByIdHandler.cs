using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetInvoiceById;

internal class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;
    private readonly BillingOptions _options;

    public GetInvoiceByIdHandler(BillingDbContext context, IOptions<BillingOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        return Result<InvoiceDto>.Success(invoice.ToDto(_options.TaxRate));
    }
}
