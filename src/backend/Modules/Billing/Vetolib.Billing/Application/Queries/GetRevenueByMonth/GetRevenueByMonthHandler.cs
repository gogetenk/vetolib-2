using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetRevenueByMonth;

internal class GetRevenueByMonthHandler
    : IRequestHandler<GetRevenueByMonthQuery, Result<IReadOnlyList<RevenueByMonthDto>>>
{
    private readonly BillingDbContext _context;

    public GetRevenueByMonthHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<RevenueByMonthDto>>> Handle(
        GetRevenueByMonthQuery query, CancellationToken ct)
    {
        // Build the last 12 months range (current month inclusive)
        var now = DateTime.UtcNow;
        var cutoff = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-11);

        // Join paid invoices with their items to sum TotalInclTax per month
        var rows = await _context.InvoiceItems
            .AsNoTracking()
            .Join(
                _context.Invoices.Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt >= cutoff),
                item => item.InvoiceId,
                invoice => invoice.Id,
                (item, invoice) => new { invoice.CreatedAt, item.TotalInclTax })
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(x => x.TotalInclTax)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync(ct);

        // Build the full 12-month range, filling zeros for months with no revenue
        var result = new List<RevenueByMonthDto>();
        for (var i = -11; i <= 0; i++)
        {
            var month = now.AddMonths(i);
            var year = month.Year;
            var monthNum = month.Month;
            var row = rows.FirstOrDefault(r => r.Year == year && r.Month == monthNum);
            result.Add(new RevenueByMonthDto(
                Month: $"{year:D4}-{monthNum:D2}",
                Total: row?.Total ?? 0m,
                Currency: "AED"));
        }

        return Result<IReadOnlyList<RevenueByMonthDto>>.Success(result);
    }
}
