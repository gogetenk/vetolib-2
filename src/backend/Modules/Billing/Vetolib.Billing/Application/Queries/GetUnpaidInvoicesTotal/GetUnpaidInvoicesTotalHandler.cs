using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetUnpaidInvoicesTotal;

internal class GetUnpaidInvoicesTotalHandler
    : IRequestHandler<GetUnpaidInvoicesTotalQuery, Result<decimal>>
{
    private readonly BillingDbContext _context;

    public GetUnpaidInvoicesTotalHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<decimal>> Handle(
        GetUnpaidInvoicesTotalQuery query, CancellationToken ct)
    {
        // Join invoices and items to sum TotalInclTax for unpaid invoices
        var total = await _context.InvoiceItems
            .AsNoTracking()
            .Join(
                _context.Invoices.Where(i =>
                    i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Draft),
                item => item.InvoiceId,
                invoice => invoice.Id,
                (item, invoice) => item.TotalInclTax)
            .SumAsync(ct);

        return Result<decimal>.Success(total);
    }
}
