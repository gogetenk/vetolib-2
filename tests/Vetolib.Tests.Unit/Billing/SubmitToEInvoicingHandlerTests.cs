using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Vetolib.Billing.Application.Commands.SubmitToEInvoicing;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Contracts.EInvoicing;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class SubmitToEInvoicingHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly IEInvoicingGateway _gateway;
    private readonly SubmitToEInvoicingHandler _handler;

    public SubmitToEInvoicingHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);

        _gateway = Substitute.For<IEInvoicingGateway>();
        _gateway.SubmitInvoiceAsync(Arg.Any<EInvoicePayload>(), Arg.Any<CancellationToken>())
            .Returns(Result<EInvoiceSubmissionResult>.Success(
                new EInvoiceSubmissionResult("MOCK-123", EInvoicingPlatformStatus.Submitted, DateTime.UtcNow)));

        var configuration = Substitute.For<IConfiguration>();
        configuration["ClinicName"].Returns("Test Clinic");
        configuration["TaxRegistrationNumber"].Returns("FR12345678901");

        var generatorFactory = new InvoicePdfGeneratorFactory();

        _handler = new SubmitToEInvoicingHandler(_context, _gateway, generatorFactory, configuration);
    }

    private Guid SeedFrenchInvoice(InvoiceStatus status = InvoiceStatus.Sent)
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

        if (status == InvoiceStatus.Sent)
            invoice.UpdateStatus(InvoiceStatus.Sent);

        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return invoice.Id;
    }

    private Guid SeedUaeInvoice(InvoiceStatus status = InvoiceStatus.Sent)
    {
        var invoiceResult = Invoice.Create(
            ClinicId, AnimalId, "INV-AE-001",
            "Consultation", 300m, 0.05m,
            currencyCode: "AED", countryCode: "AE",
            buyerName: "Ahmed Ali");

        invoiceResult.IsSuccess.Should().BeTrue();
        var invoice = invoiceResult.Value;

        if (status == InvoiceStatus.Sent)
            invoice.UpdateStatus(InvoiceStatus.Sent);

        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return invoice.Id;
    }

    [Fact]
    public async Task Handle_FrenchInvoiceSent_SubmitsSuccessfully()
    {
        var invoiceId = SeedFrenchInvoice();

        var result = await _handler.Handle(new SubmitToEInvoicingCommand(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EInvoicingStatus.Should().Be(EInvoicingPlatformStatus.Submitted);
        result.Value.PlatformInvoiceId.Should().Be("MOCK-123");
    }

    [Fact]
    public async Task Handle_NonFrenchInvoice_ReturnsError()
    {
        var invoiceId = SeedUaeInvoice();

        var result = await _handler.Handle(new SubmitToEInvoicingCommand(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EINVOICING_NOT_APPLICABLE"));
    }

    [Fact]
    public async Task Handle_DraftInvoice_ReturnsError()
    {
        var invoiceId = SeedFrenchInvoice(InvoiceStatus.Draft);

        var result = await _handler.Handle(new SubmitToEInvoicingCommand(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EINVOICING_INVALID_STATUS"));
    }

    [Fact]
    public async Task Handle_AlreadySubmitted_ReturnsError()
    {
        var invoiceId = SeedFrenchInvoice();

        // First submission
        await _handler.Handle(new SubmitToEInvoicingCommand(invoiceId), CancellationToken.None);

        // Second submission
        var result = await _handler.Handle(new SubmitToEInvoicingCommand(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EINVOICING_ALREADY_SUBMITTED"));
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(new SubmitToEInvoicingCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
