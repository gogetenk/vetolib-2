using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.ListInvoices;

internal class ListInvoicesHandler : IRequestHandler<ListInvoicesQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly BillingDbContext _context;
    private readonly BillingOptions _options;

    public ListInvoicesHandler(BillingDbContext context, IOptions<BillingOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(ListInvoicesQuery query, CancellationToken ct)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        var dtos = invoices.Select(i => i.ToDto(_options.TaxRate)).ToList();

        return Result<IReadOnlyList<InvoiceDto>>.Success(dtos);
    }
}
