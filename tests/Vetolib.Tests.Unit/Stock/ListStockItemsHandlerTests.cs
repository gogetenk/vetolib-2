using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Application;
using Vetolib.Stock.Application.Queries.ListStockItems;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

[Collection("StockTests")]
public class ListStockItemsHandlerTests : IDisposable
{
    // Must match all other Stock test classes
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly StockDbContext _context;
    private readonly ListStockItemsHandler _handler;

    public ListStockItemsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new StockDbContext(options, clinicContext, publisher);
        _handler = new ListStockItemsHandler(_context, Options.Create(new StockOptions()));
    }

    private StockItem SeedStockItem(
        string name,
        int quantity,
        string category = "Medication",
        int minThreshold = 5,
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
    public async Task Handle_WhenItemsExist_ReturnsAllItems()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 10);
        SeedStockItem("Metronidazole 200mg", quantity: 20);

        var result = await _handler.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByCategory_ReturnsOnlyMatchingItems()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 10, category: "Medication");
        SeedStockItem("Syringe 5ml", quantity: 50, category: "Supply");

        var result = await _handler.Handle(
            new ListStockItemsQuery(Category: "Medication"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Amoxicillin 250mg");
    }

    [Fact]
    public async Task Handle_WhenFilteringByInvalidCategory_ReturnsAllItems()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 10, category: "Medication");
        SeedStockItem("Syringe 5ml", quantity: 50, category: "Supply");

        var result = await _handler.Handle(
            new ListStockItemsQuery(Category: "InvalidCategory"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenLowStockFilterEnabled_ReturnsOnlyItemsBelowThreshold()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 2, minThreshold: 10);
        SeedStockItem("Metronidazole 200mg", quantity: 20, minThreshold: 5);

        var result = await _handler.Handle(
            new ListStockItemsQuery(LowStock: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Amoxicillin 250mg");
    }

    [Fact]
    public async Task Handle_WhenExpiringSoonFilterEnabled_ReturnsOnlyItemsExpiringWithin30Days()
    {
        SeedStockItem("Vaccine A", quantity: 5, expiryDate: DateTime.UtcNow.AddDays(10));
        SeedStockItem("Vaccine B", quantity: 5, expiryDate: DateTime.UtcNow.AddDays(60));
        SeedStockItem("Bandage", quantity: 100);

        var result = await _handler.Handle(
            new ListStockItemsQuery(ExpiringSoon: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Vaccine A");
    }

    [Fact]
    public async Task Handle_WhenCombiningLowStockAndCategory_ReturnsOnlyMatchingItems()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 2, category: "Medication", minThreshold: 10);
        SeedStockItem("Syringe 5ml", quantity: 2, category: "Supply", minThreshold: 10);
        SeedStockItem("Metronidazole 200mg", quantity: 20, category: "Medication", minThreshold: 5);

        var result = await _handler.Handle(
            new ListStockItemsQuery(Category: "Medication", LowStock: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Amoxicillin 250mg");
    }

    [Fact]
    public async Task Handle_ReturnedDto_ContainsCorrectFieldValues()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 15, category: "Medication", minThreshold: 5);

        var result = await _handler.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.Name.Should().Be("Amoxicillin 250mg");
        dto.Category.Should().Be("Medication");
        dto.Quantity.Should().Be(15);
        dto.MinThreshold.Should().Be(5);
        dto.Unit.Should().Be("tablet");
        dto.IsLowStock.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}
