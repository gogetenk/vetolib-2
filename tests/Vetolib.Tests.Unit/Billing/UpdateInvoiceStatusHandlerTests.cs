using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class UpdateInvoiceStatusHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnimalId = Guid.NewGuid();

    private readonly BillingDbContext _context;
    private readonly UpdateInvoiceStatusHandler _handler;

    private Guid SeededInvoiceId { get; set; }

    public UpdateInvoiceStatusHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options, clinicContext, publisher);
        _handler = new UpdateInvoiceStatusHandler(_context);

        SeedDraftInvoice();
    }

    private void SeedDraftInvoice()
    {
        var invoiceResult = Invoice.Create(
            ClinicId,
            AnimalId,
            "INV-2026-001",
            "Consultation vétérinaire",
            300m);

        invoiceResult.IsSuccess.Should().BeTrue();
        _context.Invoices.Add(invoiceResult.Value);
        _context.SaveChanges();

        SeededInvoiceId = invoiceResult.Value.Id;
    }

    private UpdateInvoiceStatusCommand BuildCommand(
        InvoiceStatus newStatus,
        Guid? invoiceId = null)
        => new(
            InvoiceId: invoiceId ?? SeededInvoiceId,
            NewStatus: newStatus);

    [Fact]
    public async Task Handle_TransitionDraftToSent_ReturnsSuccess()
    {
        var cmd = BuildCommand(InvoiceStatus.Sent);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public async Task Handle_TransitionDraftToSent_SetsDueDate()
    {
        var cmd = BuildCommand(InvoiceStatus.Sent);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DueDate.Should().NotBeNull();
        result.Value.DueDate!.Value.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_TransitionSentToPaid_ReturnsSuccess()
    {
        // Draft → Sent first
        await _handler.Handle(BuildCommand(InvoiceStatus.Sent), CancellationToken.None);

        var result = await _handler.Handle(BuildCommand(InvoiceStatus.Paid), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task Handle_TransitionDraftToCancelled_ReturnsSuccess()
    {
        var cmd = BuildCommand(InvoiceStatus.Cancelled);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_InvalidTransition_PaidToDraft_ReturnsError()
    {
        // Draft → Sent → Paid
        await _handler.Handle(BuildCommand(InvoiceStatus.Sent), CancellationToken.None);
        await _handler.Handle(BuildCommand(InvoiceStatus.Paid), CancellationToken.None);

        var result = await _handler.Handle(BuildCommand(InvoiceStatus.Draft), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_IMMUTABLE"));
    }

    [Fact]
    public async Task Handle_InvalidTransition_DraftToPaid_ReturnsError()
    {
        var result = await _handler.Handle(BuildCommand(InvoiceStatus.Paid), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFound()
    {
        var cmd = BuildCommand(InvoiceStatus.Sent, invoiceId: Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_AfterPaid_CannotTransitionToCancelled()
    {
        await _handler.Handle(BuildCommand(InvoiceStatus.Sent), CancellationToken.None);
        await _handler.Handle(BuildCommand(InvoiceStatus.Paid), CancellationToken.None);

        var result = await _handler.Handle(BuildCommand(InvoiceStatus.Cancelled), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVOICE_IMMUTABLE"));
    }

    public void Dispose() => _context.Dispose();
}
