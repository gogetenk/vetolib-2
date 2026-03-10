using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetInvoiceCount;

internal class GetInvoiceCountHandler : IRequestHandler<GetInvoiceCountQuery, Result<int>>
{
    private readonly BillingDbContext _context;

    public GetInvoiceCountHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(GetInvoiceCountQuery query, CancellationToken ct)
    {
        var count = await _context.Invoices
            .AsNoTracking()
            .CountAsync(ct);

        return Result<int>.Success(count);
    }
}
