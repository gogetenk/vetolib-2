using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Application.Queries.GetStockAlerts;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

[Collection("StockTests")]
public class GetStockAlertsHandlerTests : IDisposable
{
    // Must match all other Stock test classes
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly StockDbContext _context;
    private readonly IPublisher _publisher;
    private readonly GetStockAlertsHandler _handler;

    public GetStockAlertsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new StockDbContext(options, clinicContext, _publisher);
        _handler = new GetStockAlertsHandler(_context, _publisher);
    }

    private StockItem SeedStockItem(
        string name,
        int quantity,
        int minThreshold = 5,
        string category = "Medication",
        DateTime? expiryDate = null)
    {
        var item = StockItem.Create(
            ClinicId,
            name,
            category,
            quantity,
            "tablet",
            minThreshold,
            expiryDate: expiryDate).Value;

        _context.StockItems.Add(item);
        _context.SaveChanges();
        return item;
    }

    [Fact]
    public async Task Handle_WhenItemBelowThreshold_ReturnsItInLowStockAlerts()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 2, minThreshold: 10);

        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LowStockItems.Should().HaveCount(1);
        result.Value.LowStockItems[0].Name.Should().Be("Amoxicillin 250mg");
    }

    [Fact]
    public async Task Handle_WhenAllItemsAboveThreshold_ReturnsEmptyLowStockAlerts()
    {
        SeedStockItem("Metronidazole 200mg", quantity: 20, minThreshold: 5);

        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LowStockItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenItemExpiresWithin30Days_ReturnsItInExpiringAlerts()
    {
        var expiryDate = DateTime.UtcNow.AddDays(10);
        SeedStockItem("Vaccine A", quantity: 5, expiryDate: expiryDate);

        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiringItems.Should().HaveCount(1);
        result.Value.ExpiringItems[0].Name.Should().Be("Vaccine A");
    }

    [Fact]
    public async Task Handle_WhenItemExpiresAfter30Days_DoesNotAppearInExpiringAlerts()
    {
        var expiryDate = DateTime.UtcNow.AddDays(60);
        SeedStockItem("Vaccine B", quantity: 5, expiryDate: expiryDate);

        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiringItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenItemExpiringWithin30Days_PublishesStockExpiringEvent()
    {
        var expiryDate = DateTime.UtcNow.AddDays(5);
        SeedStockItem("Rabies Vaccine", quantity: 3, expiryDate: expiryDate);

        await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<StockExpiringEvent>(e => e.StockItemId != Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoItems_ReturnsBothListsEmpty()
    {
        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LowStockItems.Should().BeEmpty();
        result.Value.ExpiringItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenItemHasNoExpiryDate_DoesNotAppearInExpiringAlerts()
    {
        SeedStockItem("Bandage", quantity: 3, minThreshold: 10, expiryDate: null);

        var result = await _handler.Handle(new GetStockAlertsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiringItems.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
