using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class GenerateInvoicePdfHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly GenerateInvoicePdfHandler _handler;

    public GenerateInvoicePdfHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ClinicName"] = "Desert Paws Veterinary Clinic",
                ["TaxRegistrationNumber"] = "100XXXXXXXXX"
            })
            .Build();

        var factory = new InvoicePdfGeneratorFactory();

        _handler = new GenerateInvoicePdfHandler(_context, config, factory);
    }

    private async Task<Guid> SeedSentInvoiceAsync(string countryCode = "AE")
    {
        var result = Invoice.Create(
            ClinicId,
            AnimalId,
            "INV-2026-100",
            "Consultation",
            150m,
            taxRate: 0.05m,
            currencyCode: "AED",
            countryCode: countryCode);

        result.IsSuccess.Should().BeTrue();
        var invoice = result.Value;

        // Transition to Sent so PDF generation is allowed
        var statusResult = invoice.UpdateStatus(InvoiceStatus.Sent);
        statusResult.IsSuccess.Should().BeTrue();

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice.Id;
    }

    private async Task<Guid> SeedDraftInvoiceAsync()
    {
        var result = Invoice.Create(
            ClinicId,
            AnimalId,
            "INV-2026-DRAFT",
            "Consultation",
            150m,
            taxRate: 0.05m);

        result.IsSuccess.Should().BeTrue();
        var invoice = result.Value;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice.Id;
    }

    private async Task<Guid> SeedCancelledInvoiceAsync()
    {
        var result = Invoice.Create(
            ClinicId,
            AnimalId,
            "INV-2026-CANCEL",
            "Consultation",
            150m,
            taxRate: 0.05m);

        result.IsSuccess.Should().BeTrue();
        var invoice = result.Value;
        invoice.UpdateStatus(InvoiceStatus.Cancelled);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice.Id;
    }

    [Fact]
    public async Task Handle_SentInvoice_ReturnsSuccessWithPdfBytes()
    {
        var invoiceId = await SeedSentInvoiceAsync();

        var result = await _handler.Handle(new GenerateInvoicePdfQuery(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PdfBytes.Should().NotBeNull();
        result.Value.PdfBytes.Length.Should().BeGreaterThan(0);
        result.Value.InvoiceNumber.Should().Be("INV-2026-100");
    }

    [Fact]
    public async Task Handle_SentInvoice_PdfStartsWithMagicBytes()
    {
        var invoiceId = await SeedSentInvoiceAsync();

        var result = await _handler.Handle(new GenerateInvoicePdfQuery(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var header = System.Text.Encoding.ASCII.GetString(result.Value.PdfBytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public async Task Handle_NonExistentInvoice_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GenerateInvoicePdfQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_DraftInvoice_ReturnsError()
    {
        var invoiceId = await SeedDraftInvoiceAsync();

        var result = await _handler.Handle(new GenerateInvoicePdfQuery(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_DRAFT"));
    }

    [Fact]
    public async Task Handle_CancelledInvoice_ReturnsError()
    {
        var invoiceId = await SeedCancelledInvoiceAsync();

        var result = await _handler.Handle(new GenerateInvoicePdfQuery(invoiceId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_CANCELLED"));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
