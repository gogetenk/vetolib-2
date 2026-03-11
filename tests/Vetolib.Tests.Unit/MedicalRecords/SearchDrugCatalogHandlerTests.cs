using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.SearchDrugCatalog;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class SearchDrugCatalogHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly SearchDrugCatalogHandler _handler;

    public SearchDrugCatalogHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new SearchDrugCatalogHandler(_context);
    }

    private void SeedDrug(string innName, string displayName, DrugCategory category = DrugCategory.Antibiotic, bool isActive = true, Guid? clinicId = null)
    {
        var result = DrugCatalogEntry.Create(innName, displayName, category, clinicId);
        var entry = result.Value;
        if (!isActive)
            entry.Deactivate();
        _context.DrugCatalogEntries.Add(entry);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenNoDrugsExist_ReturnsEmptyList()
    {
        var query = new SearchDrugCatalogQuery(null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoSearchTerm_ReturnsAllActiveDrugs()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        SeedDrug("Meloxicam", "Metacam 5mg/ml", DrugCategory.AntiInflammatory);
        var query = new SearchDrugCatalogQuery(null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenSearchTermMatchesInnName_ReturnsMatchingDrugs()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        SeedDrug("Meloxicam", "Metacam 5mg/ml", DrugCategory.AntiInflammatory);
        var query = new SearchDrugCatalogQuery("amox");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].InnName.Should().Be("Amoxicillin");
    }

    [Fact]
    public async Task Handle_WhenSearchTermMatchesDisplayName_ReturnsMatchingDrugs()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        SeedDrug("Meloxicam", "Metacam 5mg/ml", DrugCategory.AntiInflammatory);
        var query = new SearchDrugCatalogQuery("Metacam");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].InnName.Should().Be("Meloxicam");
    }

    [Fact]
    public async Task Handle_WhenSearchTermMatchesNothing_ReturnsEmptyList()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        var query = new SearchDrugCatalogQuery("xyznomatch");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SearchIsCaseInsensitive()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        var query = new SearchDrugCatalogQuery("AMOXICILLIN");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_InactiveDrugsAreExcluded()
    {
        SeedDrug("Amoxicillin", "Amoxicillin 250mg", isActive: true);
        SeedDrug("OldDrug", "Deprecated Drug", isActive: false);
        var query = new SearchDrugCatalogQuery(null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].InnName.Should().Be("Amoxicillin");
    }

    [Fact]
    public async Task Handle_LimitDefaultIs20WhenNotSpecified()
    {
        for (int i = 0; i < 25; i++)
            SeedDrug($"Drug{i:D2}", $"Drug Display {i:D2}");

        var query = new SearchDrugCatalogQuery(null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(20);
    }

    [Fact]
    public async Task Handle_LimitIsRespectedWhenSpecified()
    {
        for (int i = 0; i < 10; i++)
            SeedDrug($"Drug{i:D2}", $"Drug Display {i:D2}");

        var query = new SearchDrugCatalogQuery(null, Limit: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_LimitOutOfRange_DefaultsTo20()
    {
        for (int i = 0; i < 25; i++)
            SeedDrug($"Drug{i:D2}", $"Drug Display {i:D2}");

        // Limit of 0 is invalid, should default to 20
        var query = new SearchDrugCatalogQuery(null, Limit: 0);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(20);
    }

    [Fact]
    public async Task Handle_ResultsOrderedByInnNameAlphabetically()
    {
        SeedDrug("Zolazepam", "Telazol");
        SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        SeedDrug("Meloxicam", "Metacam 5mg/ml", DrugCategory.AntiInflammatory);

        var query = new SearchDrugCatalogQuery(null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].InnName.Should().Be("Amoxicillin");
        result.Value[1].InnName.Should().Be("Meloxicam");
        result.Value[2].InnName.Should().Be("Zolazepam");
    }

    public void Dispose() => _context.Dispose();
}
