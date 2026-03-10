using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Application.Commands.DecrementStockForPrescription;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class DecrementStockForPrescriptionHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("33333333-3333-3333-3333-333333333333");

    private readonly StockDbContext _context;
    private readonly IPublisher _publisher;
    private readonly DecrementStockForPrescriptionHandler _handler;

    public DecrementStockForPrescriptionHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new StockDbContext(options, clinicContext, _publisher);
        _handler = new DecrementStockForPrescriptionHandler(_context, _publisher);
    }

    private StockItem SeedStockItem(int quantity, int minThreshold = 5, Guid? drugCatalogEntryId = null)
    {
        var item = StockItem.Create(
            ClinicId,
            "Doxycycline 100mg",
            "Medication",
            quantity,
            "capsule",
            minThreshold,
            expiryDate: null,
            drugCatalogEntryId: drugCatalogEntryId).Value;

        _context.StockItems.Add(item);
        _context.SaveChanges();
        return item;
    }

    [Fact]
    public async Task Handle_WhenStockItemExists_DecrementsQuantity()
    {
        var item = SeedStockItem(quantity: 20);
        var prescriptionId = Guid.NewGuid();
        var cmd = new DecrementStockForPrescriptionCommand(item.Id, 3, prescriptionId, ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var updated = await _context.StockItems.FindAsync(item.Id);
        updated!.Quantity.Should().Be(17);
    }

    [Fact]
    public async Task Handle_WhenStockItemExists_CreatesMovementWithPrescriptionReason()
    {
        var item = SeedStockItem(quantity: 20);
        var prescriptionId = Guid.NewGuid();
        var cmd = new DecrementStockForPrescriptionCommand(item.Id, 5, prescriptionId, ClinicId);

        await _handler.Handle(cmd, CancellationToken.None);

        var movement = _context.StockMovements.FirstOrDefault(m => m.StockItemId == item.Id);
        movement.Should().NotBeNull();
        movement!.Reason.Should().Be($"Prescription #{prescriptionId}");
        movement.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenStockNotFound_ReturnsNotFound()
    {
        var cmd = new DecrementStockForPrescriptionCommand(Guid.NewGuid(), 1, Guid.NewGuid(), ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ReturnsError()
    {
        var item = SeedStockItem(quantity: 2);
        var cmd = new DecrementStockForPrescriptionCommand(item.Id, 10, Guid.NewGuid(), ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_WhenStockDropsBelowThreshold_PublishesStockLowEvent()
    {
        // quantity=4, minThreshold=5 — after decrement of 1, quantity=3 < 5 → low
        var item = SeedStockItem(quantity: 4, minThreshold: 5);
        var cmd = new DecrementStockForPrescriptionCommand(item.Id, 1, Guid.NewGuid(), ClinicId);

        await _handler.Handle(cmd, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<StockLowEvent>(e =>
                e.StockItemId == item.Id &&
                e.ClinicId == ClinicId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockRemainsAboveThreshold_DoesNotPublishStockLowEvent()
    {
        // quantity=20, minThreshold=5 — after decrement stays well above
        var item = SeedStockItem(quantity: 20, minThreshold: 5);
        var cmd = new DecrementStockForPrescriptionCommand(item.Id, 1, Guid.NewGuid(), ClinicId);

        await _handler.Handle(cmd, CancellationToken.None);

        await _publisher.DidNotReceive().Publish(
            Arg.Any<StockLowEvent>(),
            Arg.Any<CancellationToken>());
    }

    public void Dispose() => _context.Dispose();
}
