using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Commands.SubmitEReporting;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class SubmitEReportingHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private readonly BillingDbContext _context;
    private readonly IEReportingGateway _gateway;
    private readonly IClinicContext _clinicContext;
    private readonly SubmitEReportingHandler _handler;

    public SubmitEReportingHandlerTests()
    {
        _clinicContext = Substitute.For<IClinicContext>();
        _clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, _clinicContext, publisher);
        _gateway = Substitute.For<IEReportingGateway>();
        _handler = new SubmitEReportingHandler(_context, _gateway, _clinicContext);
    }

    private async Task SeedPaidFrenchB2CInvoiceAsync(decimal unitPrice = 100m, decimal taxRate = 0.20m)
    {
        var invoiceResult = Invoice.Create(
            ClinicId,
            Guid.NewGuid(),
            $"INV-{Guid.NewGuid():N}".Substring(0, 15),
            "Consultation",
            unitPrice,
            taxRate,
            countryCode: "FR",
            buyerName: "Jean Dupont",
            sellerSiren: "123456789",
            sellerVatNumber: "FR12345678901",
            operationType: OperationType.Service);

        invoiceResult.IsSuccess.Should().BeTrue();
        var invoice = invoiceResult.Value;
        invoice.UpdateStatus(InvoiceStatus.Sent);
        invoice.UpdateStatus(InvoiceStatus.Paid);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_HappyPath_SubmitsAndPersistsPeriod()
    {
        await SeedPaidFrenchB2CInvoiceAsync(unitPrice: 100m, taxRate: 0.20m);

        _gateway.SubmitEReportingAsync(
            Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(),
            Arg.Any<IReadOnlyList<EReportingTaxBreakdownDto>>(),
            Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("SUBMISSION-123"));

        var command = new SubmitEReportingCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EReportingStatus.Submitted);
        result.Value.TransactionCount.Should().Be(1);
        result.Value.TotalExclTax.Should().Be(100m);
        result.Value.TotalTax.Should().Be(20m);
        result.Value.TotalInclTax.Should().Be(120m);
        result.Value.SubmittedAt.Should().NotBeNull();

        // Verify persisted
        var persisted = await _context.EReportingPeriods.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AlreadySubmittedPeriod_ReturnsConflict()
    {
        await SeedPaidFrenchB2CInvoiceAsync();

        // Create an existing submitted period
        var existingResult = EReportingPeriod.Create(
            ClinicId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            [new EReportingTaxBreakdownData(0.20m, TaxCategory.Standard, 100m, 20m, 1)],
            1, 100m, 20m, 120m);
        existingResult.IsSuccess.Should().BeTrue();
        existingResult.Value.MarkSubmitted("EXISTING-ID");
        _context.EReportingPeriods.Add(existingResult.Value);
        await _context.SaveChangesAsync();

        var command = new SubmitEReportingCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_NoB2CData_ReturnsError()
    {
        // No invoices seeded

        var command = new SubmitEReportingCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("NO_DATA"));
    }

    [Fact]
    public async Task Handle_GatewayFails_ReturnsError()
    {
        await SeedPaidFrenchB2CInvoiceAsync();

        _gateway.SubmitEReportingAsync(
            Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(),
            Arg.Any<IReadOnlyList<EReportingTaxBreakdownDto>>(),
            Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<string>.Error("Platform unreachable"));

        var command = new SubmitEReportingCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("GATEWAY_ERROR"));
    }

    [Fact]
    public async Task Handle_InvalidPeriod_ReturnsInvalid()
    {
        var command = new SubmitEReportingCommand(
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1)); // End before start

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose() => _context.Dispose();
}
