using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application;
using Vetolib.Billing.Application.Commands.AddInvoiceItem;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class AddInvoiceItemHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly AddInvoiceItemHandler _handler;

    private Guid SeededInvoiceId { get; set; }

    public AddInvoiceItemHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new AddInvoiceItemHandler(_context, new CountryTaxResolver());

        SeedDraftInvoice();
    }

    private void SeedDraftInvoice()
    {
        var invoiceResult = Invoice.Create(
            ClinicId,
            AnimalId,
            "INV-2026-001",
            "Consultation initiale",
            200m,
            0.05m);

        invoiceResult.IsSuccess.Should().BeTrue();
        _context.Invoices.Add(invoiceResult.Value);
        _context.SaveChanges();

        SeededInvoiceId = invoiceResult.Value.Id;
    }

    private AddInvoiceItemCommand BuildCommand(
        Guid? invoiceId = null,
        string description = "Vaccin antirabique",
        decimal unitPrice = 80m,
        TaxCategory taxCategory = TaxCategory.Standard)
        => new(
            InvoiceId: invoiceId ?? SeededInvoiceId,
            Description: description,
            UnitPrice: unitPrice,
            TaxCategory: taxCategory);

    [Fact]
    public async Task Handle_HappyPath_ReturnsSuccess()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_HappyPath_ItemAddedToInvoice()
    {
        var cmd = BuildCommand(description: "Prise de sang", unitPrice: 120m);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // Initial item + newly added item = 2
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(i => i.Description == "Prise de sang");
    }

    [Fact]
    public async Task Handle_HappyPath_TotalRecalculated()
    {
        var initialResult = await _handler.Handle(
            BuildCommand(description: "Vaccin", unitPrice: 100m), CancellationToken.None);

        initialResult.IsSuccess.Should().BeTrue();
        // Initial 200 + new 100 = 300 excl. tax, total incl. 5% VAT = 315
        initialResult.Value.Total.Should().Be(315m);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFound()
    {
        var cmd = BuildCommand(invoiceId: Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenInvoiceIsPaid_ReturnsError()
    {
        // Transition Draft -> Sent -> Paid
        await TransitionInvoice(SeededInvoiceId, InvoiceStatus.Sent);
        await TransitionInvoice(SeededInvoiceId, InvoiceStatus.Paid);

        var cmd = BuildCommand(description: "Extra item", unitPrice: 50m);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_IMMUTABLE"));
    }

    [Fact]
    public async Task Handle_WhenInvoiceIsCancelled_ReturnsError()
    {
        await TransitionInvoice(SeededInvoiceId, InvoiceStatus.Cancelled);

        var cmd = BuildCommand(description: "Extra item", unitPrice: 50m);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_CANCELLED"));
    }

    [Fact]
    public async Task Handle_MixedTaxCategories_CorrectTotals()
    {
        // Seeded invoice has 200 @ 5% = 210
        // Add item with Zero tax
        var cmd = BuildCommand(description: "Exempt service", unitPrice: 100m, taxCategory: TaxCategory.Zero);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // 200 + 100 = 300 subtotal, tax = 10 (from first) + 0 (from second) = 10
        result.Value.Subtotal.Should().Be(300m);
        result.Value.VatAmount.Should().Be(10m);
        result.Value.Total.Should().Be(310m);
    }

    [Fact]
    public async Task Handle_NewItem_HasCorrectTaxCategoryInDto()
    {
        var cmd = BuildCommand(description: "Zero-rated item", unitPrice: 50m, taxCategory: TaxCategory.Zero);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var newItem = result.Value.Items.First(i => i.Description == "Zero-rated item");
        newItem.TaxCategory.Should().Be(TaxCategory.Zero);
        newItem.TaxRate.Should().Be(0m);
    }

    private async Task TransitionInvoice(Guid invoiceId, InvoiceStatus newStatus)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstAsync(i => i.Id == invoiceId);

        invoice.UpdateStatus(newStatus);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
