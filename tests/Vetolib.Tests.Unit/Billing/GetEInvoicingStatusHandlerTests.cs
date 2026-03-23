using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Queries.GetEInvoicingStatus;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Contracts.EInvoicing;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class GetEInvoicingStatusHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly IEInvoicingGateway _gateway;
    private readonly GetEInvoicingStatusHandler _handler;

    public GetEInvoicingStatusHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);

        _gateway = Substitute.For<IEInvoicingGateway>();

        _handler = new GetEInvoicingStatusHandler(_context, _gateway);
    }

    private Guid SeedSubmittedFrenchInvoice()
    {
        var invoiceResult = Invoice.Create(
            ClinicId, AnimalId, "INV-FR-001",
            "Consultation", 100m, 0.20m,
            currencyCode: "EUR", countryCode: "FR",
            buyerName: "Jean Dupont",
            sellerSiren: "123456789",
            sellerVatNumber: "FR12345678901",
            operationType: OperationType.Service);

        invoiceResult.IsSuccess.Should().BeTrue();
        var invoice = invoiceResult.Value;
        invoice.UpdateStatus(InvoiceStatus.Sent);
        invoice.MarkEInvoicingSubmitted("PLATFORM-001", EInvoicingPlatformStatus.Submitted);

        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return invoice.Id;
    }

    [Fact]
    public async Task Handle_SubmittedInvoice_ReturnsStatus()
    {
        var invoiceId = SeedSubmittedFrenchInvoice();
        _gateway.GetStatusAsync("PLATFORM-001", Arg.Any<CancellationToken>())
            .Returns(Result<EInvoiceStatus>.Success(
                new EInvoiceStatus("PLATFORM-001", EInvoicingPlatformStatus.Accepted, null)));

        var result = await _handler.Handle(new GetEInvoicingStatusQuery(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(EInvoicingPlatformStatus.Accepted);
        result.Value.PlatformInvoiceId.Should().Be("PLATFORM-001");
    }

    [Fact]
    public async Task Handle_NotSubmitted_ReturnsError()
    {
        var invoiceResult = Invoice.Create(
            ClinicId, AnimalId, "INV-FR-002",
            "Consultation", 100m, 0.20m,
            currencyCode: "EUR", countryCode: "FR",
            buyerName: "Jean Dupont",
            sellerSiren: "123456789",
            sellerVatNumber: "FR12345678901",
            operationType: OperationType.Service);

        _context.Invoices.Add(invoiceResult.Value);
        _context.SaveChanges();

        var result = await _handler.Handle(
            new GetEInvoicingStatusQuery(invoiceResult.Value.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EINVOICING_NOT_SUBMITTED"));
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetEInvoicingStatusQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_StatusChanged_UpdatesStoredStatus()
    {
        var invoiceId = SeedSubmittedFrenchInvoice();
        _gateway.GetStatusAsync("PLATFORM-001", Arg.Any<CancellationToken>())
            .Returns(Result<EInvoiceStatus>.Success(
                new EInvoiceStatus("PLATFORM-001", EInvoicingPlatformStatus.Accepted, null)));

        await _handler.Handle(new GetEInvoicingStatusQuery(invoiceId), CancellationToken.None);

        // Verify the stored status was updated
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        invoice!.EInvoicingStatus.Should().Be(EInvoicingPlatformStatus.Accepted);
    }

    public void Dispose() => _context.Dispose();
}
