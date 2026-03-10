using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Application.Queries.CheckStockAvailability;
using Vetolib.Stock.Contracts;
using Vetolib.Stock.Domain;
using Vetolib.Stock.Infrastructure;
using Xunit;

namespace Vetolib.Tests.Unit.Stock;

public class CheckStockAvailabilityHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DrugCatalogEntryId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly StockDbContext _context;
    private readonly CheckStockAvailabilityHandler _handler;

    public CheckStockAvailabilityHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new StockDbContext(options, clinicContext, publisher);
        _handler = new CheckStockAvailabilityHandler(_context);
    }

    private StockItem SeedStockItem(
        string name,
        int quantity,
        string category = "Medication",
        Guid? drugCatalogEntryId = null,
        int minThreshold = 5)
    {
        var item = StockItem.Create(
            ClinicId,
            name,
            category,
            quantity,
            "tablet",
            minThreshold,
            expiryDate: null,
            drugCatalogEntryId: drugCatalogEntryId).Value;

        _context.StockItems.Add(item);
        _context.SaveChanges();
        return item;
    }

    [Fact]
    public async Task Handle_WhenStockAvailable_ReturnsAvailableTrue()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 20, drugCatalogEntryId: DrugCatalogEntryId);

        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Available.Should().BeTrue();
        result.Value.Quantity.Should().Be(20);
        result.Value.Unit.Should().Be("tablet");
        result.Value.Alternatives.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoStockForDrug_ReturnsAvailableFalse()
    {
        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Available.Should().BeFalse();
        result.Value.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenOutOfStock_ReturnsAvailableFalse()
    {
        SeedStockItem("Amoxicillin 250mg", quantity: 0, drugCatalogEntryId: DrugCatalogEntryId);

        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Available.Should().BeFalse();
        result.Value.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenOutOfStock_ReturnsAlternativesFromSameCategory()
    {
        // Primary item — out of stock
        SeedStockItem("Amoxicillin 250mg", quantity: 0, category: "Medication", drugCatalogEntryId: DrugCatalogEntryId);

        // Alternative — same category, in stock, linked to catalog
        var altDrugId = Guid.NewGuid();
        SeedStockItem("Amoxicillin 500mg", quantity: 10, category: "Medication", drugCatalogEntryId: altDrugId);

        // Unrelated — different category, should not appear
        SeedStockItem("Bandage", quantity: 50, category: "Supply", drugCatalogEntryId: Guid.NewGuid());

        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Available.Should().BeFalse();
        result.Value.Alternatives.Should().HaveCount(1);
        result.Value.Alternatives[0].Name.Should().Be("Amoxicillin 500mg");
        result.Value.Alternatives[0].Quantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenLowStock_ReturnsIsLowStockTrue()
    {
        // quantity=3, minThreshold=5 → IsLowStock
        SeedStockItem("Metronidazole 200mg", quantity: 3, drugCatalogEntryId: DrugCatalogEntryId, minThreshold: 5);

        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Available.Should().BeTrue();
        result.Value.IsLowStock.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AlternativesAreExcludedIfNoLinkedCatalogEntry()
    {
        // Primary — out of stock
        SeedStockItem("Drug A", quantity: 0, category: "Medication", drugCatalogEntryId: DrugCatalogEntryId);

        // Supply item without DrugCatalogEntryId — must not appear as alternative
        SeedStockItem("Syringe", quantity: 100, category: "Medication", drugCatalogEntryId: null);

        var query = new CheckStockAvailabilityQuery(DrugCatalogEntryId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Alternatives.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
