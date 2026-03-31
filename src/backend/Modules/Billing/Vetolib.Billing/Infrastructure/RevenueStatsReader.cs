using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;

namespace Vetolib.Billing.Infrastructure;

internal class RevenueStatsReader : IRevenueStatsReader
{
    private readonly BillingDbContext _context;

    public RevenueStatsReader(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyDictionary<Guid, decimal>>> GetRevenueByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default)
    {
        if (clinicIds.Count == 0)
            return Result<IReadOnlyDictionary<Guid, decimal>>.Success(
                new Dictionary<Guid, decimal>());

        var revenues = await _context.Invoices
            .IgnoreQueryFilters()
            .Where(i => clinicIds.Contains(i.ClinicId) && i.Status == InvoiceStatus.Paid)
            .Include(i => i.Items)
            .GroupBy(i => i.ClinicId)
            .Select(g => new
            {
                ClinicId = g.Key,
                Total = g.SelectMany(i => i.Items).Sum(item => item.TotalInclTax)
            })
            .ToListAsync(ct);

        var result = clinicIds.ToDictionary(
            id => id,
            id => revenues.FirstOrDefault(r => r.ClinicId == id)?.Total ?? 0m);

        return Result<IReadOnlyDictionary<Guid, decimal>>.Success(result);
    }
}
