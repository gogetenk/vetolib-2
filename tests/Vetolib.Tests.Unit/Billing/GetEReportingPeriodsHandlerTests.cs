using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Queries.GetEReportingPeriods;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class GetEReportingPeriodsHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private readonly BillingDbContext _context;
    private readonly GetEReportingPeriodsHandler _handler;

    public GetEReportingPeriodsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new GetEReportingPeriodsHandler(_context);
    }

    [Fact]
    public async Task Handle_NoPeriods_ReturnsEmptyList()
    {
        var query = new GetEReportingPeriodsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPeriods_ReturnsAllPeriods()
    {
        var period1 = EReportingPeriod.Create(
            ClinicId,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            [new EReportingTaxBreakdownData(0.20m, TaxCategory.Standard, 1000m, 200m, 10)],
            10, 1000m, 200m, 1200m);
        period1.IsSuccess.Should().BeTrue();

        var period2 = EReportingPeriod.Create(
            ClinicId,
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28),
            [new EReportingTaxBreakdownData(0.20m, TaxCategory.Standard, 2000m, 400m, 20)],
            20, 2000m, 400m, 2400m);
        period2.IsSuccess.Should().BeTrue();

        _context.EReportingPeriods.AddRange(period1.Value, period2.Value);
        await _context.SaveChangesAsync();

        var query = new GetEReportingPeriodsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithPeriods_OrderedByPeriodEndDescending()
    {
        var period1 = EReportingPeriod.Create(
            ClinicId,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            [new EReportingTaxBreakdownData(0.20m, TaxCategory.Standard, 1000m, 200m, 10)],
            10, 1000m, 200m, 1200m);

        var period2 = EReportingPeriod.Create(
            ClinicId,
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28),
            [new EReportingTaxBreakdownData(0.20m, TaxCategory.Standard, 2000m, 400m, 20)],
            20, 2000m, 400m, 2400m);

        _context.EReportingPeriods.AddRange(period1.Value, period2.Value);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetEReportingPeriodsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].PeriodEnd.Should().Be(new DateOnly(2026, 2, 28));
        result.Value[1].PeriodEnd.Should().Be(new DateOnly(2026, 1, 31));
    }

    [Fact]
    public async Task Handle_IncludesTaxBreakdowns()
    {
        var breakdowns = new List<EReportingTaxBreakdownData>
        {
            new(0.20m, TaxCategory.Standard, 500m, 100m, 5),
            new(0.10m, TaxCategory.Reduced, 300m, 30m, 3)
        };

        var period = EReportingPeriod.Create(
            ClinicId,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            breakdowns, 8, 800m, 130m, 930m);

        _context.EReportingPeriods.Add(period.Value);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetEReportingPeriodsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].TaxBreakdowns.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();
}
