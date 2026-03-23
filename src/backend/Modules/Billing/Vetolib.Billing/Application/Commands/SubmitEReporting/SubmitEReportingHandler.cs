using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Application.Commands.SubmitEReporting;

internal class SubmitEReportingHandler
    : IRequestHandler<SubmitEReportingCommand, Result<EReportingPeriodDto>>
{
    private readonly BillingDbContext _context;
    private readonly IEReportingGateway _gateway;
    private readonly IClinicContext _clinicContext;

    public SubmitEReportingHandler(
        BillingDbContext context,
        IEReportingGateway gateway,
        IClinicContext clinicContext)
    {
        _context = context;
        _gateway = gateway;
        _clinicContext = clinicContext;
    }

    public async Task<Result<EReportingPeriodDto>> Handle(
        SubmitEReportingCommand command, CancellationToken ct)
    {
        if (command.PeriodEnd < command.PeriodStart)
            return Result<EReportingPeriodDto>.Invalid(
                new ValidationError(nameof(command.PeriodEnd), "PeriodEnd must be >= PeriodStart"));

        // Check for existing submission for this period
        var existingPeriod = await _context.EReportingPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.PeriodStart == command.PeriodStart &&
                p.PeriodEnd == command.PeriodEnd &&
                (p.Status == EReportingStatus.Submitted || p.Status == EReportingStatus.Accepted),
                ct);

        if (existingPeriod is not null)
            return Result<EReportingPeriodDto>.Conflict(
                "ALREADY_SUBMITTED:An e-reporting submission already exists for this period");

        // Aggregate B2C data
        var startDate = command.PeriodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDate = command.PeriodEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var b2cInvoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.BuyerSiren == null)
            .Where(i => i.CountryCode == "FR")
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
            .ToListAsync(ct);

        if (b2cInvoices.Count == 0)
            return Result<EReportingPeriodDto>.Error(
                "NO_DATA:No B2C invoices found for the specified period");

        // Aggregate by TaxRate + TaxCategory
        var breakdownData = b2cInvoices
            .SelectMany(inv => inv.Items)
            .GroupBy(item => new { item.TaxRate, item.TaxCategory })
            .Select(g => new EReportingTaxBreakdownData(
                g.Key.TaxRate,
                g.Key.TaxCategory,
                g.Sum(i => i.UnitPriceExclTax),
                g.Sum(i => i.TaxAmount),
                g.Count()))
            .ToList();

        var totalExclTax = breakdownData.Sum(b => b.BaseAmount);
        var totalTax = breakdownData.Sum(b => b.TaxAmount);
        var totalInclTax = totalExclTax + totalTax;

        // Create the EReportingPeriod entity
        var periodResult = EReportingPeriod.Create(
            _clinicContext.ClinicId,
            command.PeriodStart,
            command.PeriodEnd,
            breakdownData,
            b2cInvoices.Count,
            totalExclTax,
            totalTax,
            totalInclTax);

        if (!periodResult.IsSuccess)
            return Result<EReportingPeriodDto>.Invalid(periodResult.ValidationErrors.ToList());

        var period = periodResult.Value;

        // Build DTOs for gateway submission
        var breakdownDtos = period.TaxBreakdowns
            .Select(b => b.ToDto())
            .ToList()
            .AsReadOnly();

        // Submit to gateway
        var gatewayResult = await _gateway.SubmitEReportingAsync(
            _clinicContext.ClinicId,
            command.PeriodStart,
            command.PeriodEnd,
            breakdownDtos,
            b2cInvoices.Count,
            totalExclTax,
            totalTax,
            totalInclTax,
            ct);

        if (!gatewayResult.IsSuccess)
            return Result<EReportingPeriodDto>.Error(
                $"GATEWAY_ERROR:{string.Join(", ", gatewayResult.Errors)}");

        // Mark as submitted
        var submitResult = period.MarkSubmitted(gatewayResult.Value);
        if (!submitResult.IsSuccess)
            return Result<EReportingPeriodDto>.Error(string.Join(", ", submitResult.Errors));

        _context.EReportingPeriods.Add(period);
        await _context.SaveChangesAsync(ct);

        return Result<EReportingPeriodDto>.Success(period.ToDto());
    }
}
