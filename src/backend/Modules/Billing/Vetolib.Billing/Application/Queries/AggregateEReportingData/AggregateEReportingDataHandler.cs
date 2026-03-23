using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.AggregateEReportingData;

internal class AggregateEReportingDataHandler
    : IRequestHandler<AggregateEReportingDataQuery, Result<EReportingPeriodDto>>
{
    private readonly BillingDbContext _context;

    public AggregateEReportingDataHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EReportingPeriodDto>> Handle(
        AggregateEReportingDataQuery query, CancellationToken ct)
    {
        if (query.PeriodEnd < query.PeriodStart)
            return Result<EReportingPeriodDto>.Invalid(
                new ValidationError(nameof(query.PeriodEnd), "PeriodEnd must be >= PeriodStart"));

        var startDate = query.PeriodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDate = query.PeriodEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        // Filter B2C invoices: BuyerSiren is null (individuals, not businesses),
        // CountryCode is FR, Status is Paid
        var b2cInvoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.BuyerSiren == null)
            .Where(i => i.CountryCode == "FR")
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
            .ToListAsync(ct);

        if (b2cInvoices.Count == 0)
        {
            return Result<EReportingPeriodDto>.Success(new EReportingPeriodDto(
                Guid.Empty,
                query.PeriodStart,
                query.PeriodEnd,
                EReportingStatus.Draft,
                null,
                0, 0m, 0m, 0m,
                Array.Empty<EReportingTaxBreakdownDto>()));
        }

        // Aggregate by TaxRate + TaxCategory
        var breakdowns = b2cInvoices
            .SelectMany(inv => inv.Items)
            .GroupBy(item => new { item.TaxRate, item.TaxCategory })
            .Select(g => new EReportingTaxBreakdownDto(
                g.Key.TaxRate,
                g.Key.TaxCategory,
                g.Sum(i => i.UnitPriceExclTax),
                g.Sum(i => i.TaxAmount),
                g.Count()))
            .ToList()
            .AsReadOnly();

        var totalExclTax = breakdowns.Sum(b => b.BaseAmount);
        var totalTax = breakdowns.Sum(b => b.TaxAmount);
        var totalInclTax = totalExclTax + totalTax;

        return Result<EReportingPeriodDto>.Success(new EReportingPeriodDto(
            Guid.Empty, // Not persisted yet — just aggregated data
            query.PeriodStart,
            query.PeriodEnd,
            EReportingStatus.Draft,
            null,
            b2cInvoices.Count,
            totalExclTax,
            totalTax,
            totalInclTax,
            breakdowns));
    }
}
