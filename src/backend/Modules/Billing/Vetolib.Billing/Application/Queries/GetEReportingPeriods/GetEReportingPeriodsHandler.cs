using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GetEReportingPeriods;

internal class GetEReportingPeriodsHandler
    : IRequestHandler<GetEReportingPeriodsQuery, Result<IReadOnlyList<EReportingPeriodDto>>>
{
    private readonly BillingDbContext _context;

    public GetEReportingPeriodsHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<EReportingPeriodDto>>> Handle(
        GetEReportingPeriodsQuery query, CancellationToken ct)
    {
        var periods = await _context.EReportingPeriods
            .AsNoTracking()
            .Include(p => p.TaxBreakdowns)
            .OrderByDescending(p => p.PeriodEnd)
            .ToListAsync(ct);

        var dtos = periods
            .Select(p => p.ToDto())
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<EReportingPeriodDto>>.Success(dtos);
    }
}
