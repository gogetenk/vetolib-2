using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.ListInvoices;

internal class ListInvoicesHandler : IRequestHandler<ListInvoicesQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly BillingDbContext _context;

    public ListInvoicesHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(ListInvoicesQuery query, CancellationToken ct)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = invoices.Select(i => i.ToDto()).ToList();

        return Result<IReadOnlyList<InvoiceDto>>.Success(dtos);
    }
}
