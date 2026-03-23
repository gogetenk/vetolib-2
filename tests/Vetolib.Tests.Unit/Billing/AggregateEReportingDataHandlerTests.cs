using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Queries.AggregateEReportingData;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class AggregateEReportingDataHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private readonly BillingDbContext _context;
    private readonly AggregateEReportingDataHandler _handler;

    public AggregateEReportingDataHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new AggregateEReportingDataHandler(_context);
    }

    private async Task SeedInvoiceAsync(
        string countryCode = "FR",
        InvoiceStatus status = InvoiceStatus.Paid,
        string? buyerSiren = null,
        decimal unitPrice = 100m,
        decimal taxRate = 0.20m,
        TaxCategory taxCategory = TaxCategory.Standard,
        DateTime? createdAt = null)
    {
        var invoiceResult = Invoice.Create(
            ClinicId,
            Guid.NewGuid(),
            $"INV-{Guid.NewGuid():N}".Substring(0, 15),
            "Consultation",
            unitPrice,
            taxRate,
            countryCode: countryCode,
            buyerName: "Test Owner",
            sellerSiren: countryCode == "FR" ? "123456789" : null,
            sellerVatNumber: countryCode == "FR" ? "FR12345678901" : null,
            buyerSiren: buyerSiren,
            operationType: countryCode == "FR" ? OperationType.Service : null,
            itemTaxCategory: taxCategory);

        invoiceResult.IsSuccess.Should().BeTrue();
        var invoice = invoiceResult.Value;

        // Set status through valid transitions
        if (status == InvoiceStatus.Paid || status == InvoiceStatus.Sent)
        {
            invoice.UpdateStatus(InvoiceStatus.Sent);
            if (status == InvoiceStatus.Paid)
                invoice.UpdateStatus(InvoiceStatus.Paid);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Override CreatedAt if specified (after SaveChanges to avoid EF interference)
        if (createdAt.HasValue)
        {
            _context.Entry(invoice).Property(i => i.CreatedAt).CurrentValue = createdAt.Value;
            await _context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Handle_WithB2CFrenchPaidInvoices_AggregatesCorrectly()
    {
        // Two invoices with 20% standard rate
        await SeedInvoiceAsync(unitPrice: 100m, taxRate: 0.20m);
        await SeedInvoiceAsync(unitPrice: 200m, taxRate: 0.20m);

        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(2);
        result.Value.TotalExclTax.Should().Be(300m);
        result.Value.TotalTax.Should().Be(60m); // 300 * 0.20
        result.Value.TotalInclTax.Should().Be(360m);
        result.Value.TaxBreakdowns.Should().HaveCount(1);
        result.Value.TaxBreakdowns[0].TaxRate.Should().Be(0.20m);
        result.Value.TaxBreakdowns[0].BaseAmount.Should().Be(300m);
    }

    [Fact]
    public async Task Handle_WithMultipleTaxRates_GroupsByRate()
    {
        await SeedInvoiceAsync(unitPrice: 100m, taxRate: 0.20m, taxCategory: TaxCategory.Standard);
        await SeedInvoiceAsync(unitPrice: 100m, taxRate: 0.10m, taxCategory: TaxCategory.Reduced);

        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(2);
        result.Value.TaxBreakdowns.Should().HaveCount(2);

        var standard = result.Value.TaxBreakdowns.First(b => b.TaxRate == 0.20m);
        standard.BaseAmount.Should().Be(100m);
        standard.TaxAmount.Should().Be(20m);

        var reduced = result.Value.TaxBreakdowns.First(b => b.TaxRate == 0.10m);
        reduced.BaseAmount.Should().Be(100m);
        reduced.TaxAmount.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_FiltersOutB2BInvoices()
    {
        // B2C invoice (no BuyerSiren)
        await SeedInvoiceAsync(buyerSiren: null, unitPrice: 100m);
        // B2B invoice (has BuyerSiren) — should be excluded
        await SeedInvoiceAsync(buyerSiren: "987654321", unitPrice: 500m);

        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(1);
        result.Value.TotalExclTax.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_FiltersOutNonFrenchInvoices()
    {
        // French B2C invoice
        await SeedInvoiceAsync(countryCode: "FR", unitPrice: 100m);
        // UAE B2C invoice — should be excluded
        await SeedInvoiceAsync(countryCode: "AE", unitPrice: 200m, taxRate: 0.05m);

        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(1);
        result.Value.TotalExclTax.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_FiltersOutUnpaidInvoices()
    {
        // Paid invoice
        await SeedInvoiceAsync(status: InvoiceStatus.Paid, unitPrice: 100m);
        // Draft invoice — should be excluded
        await SeedInvoiceAsync(status: InvoiceStatus.Draft, unitPrice: 300m);

        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(1);
        result.Value.TotalExclTax.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_NoInvoices_ReturnsEmptyResult()
    {
        var query = new AggregateEReportingDataQuery(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionCount.Should().Be(0);
        result.Value.TotalExclTax.Should().Be(0m);
        result.Value.TotalTax.Should().Be(0m);
        result.Value.TotalInclTax.Should().Be(0m);
        result.Value.TaxBreakdowns.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidPeriod_ReturnsInvalid()
    {
        var query = new AggregateEReportingDataQuery(
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1));  // End before start

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose() => _context.Dispose();
}
