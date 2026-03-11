using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.GetDrugCatalogEntryById;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class GetDrugCatalogEntryByIdHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly GetDrugCatalogEntryByIdHandler _handler;

    public GetDrugCatalogEntryByIdHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new GetDrugCatalogEntryByIdHandler(_context);
    }

    private DrugCatalogEntry SeedGlobalDrug(string innName = "Amoxicillin", string displayName = "Amoxicillin 250mg")
    {
        var result = DrugCatalogEntry.Create(innName, displayName, DrugCategory.Antibiotic, clinicId: null);
        var entry = result.Value;
        _context.DrugCatalogEntries.Add(entry);
        _context.SaveChanges();
        return entry;
    }

    [Fact]
    public async Task Handle_WhenEntryExists_ReturnsSuccess()
    {
        var seeded = SeedGlobalDrug();
        var query = new GetDrugCatalogEntryByIdQuery(seeded.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(seeded.Id);
        result.Value.InnName.Should().Be("Amoxicillin");
        result.Value.DisplayName.Should().Be("Amoxicillin 250mg");
        result.Value.Category.Should().Be(DrugCategory.Antibiotic);
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ReturnsNotFound()
    {
        var query = new GetDrugCatalogEntryByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenEntryExists_ReturnsDtoWithCorrectId()
    {
        var seeded = SeedGlobalDrug("Meloxicam", "Metacam 5mg/ml");
        var query = new GetDrugCatalogEntryByIdQuery(seeded.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(seeded.Id);
        result.Value.InnName.Should().Be("Meloxicam");
    }

    public void Dispose() => _context.Dispose();
}
