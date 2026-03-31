using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.GetDrugAlternatives;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class GetDrugAlternativesHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");

    private readonly MedicalRecordsDbContext _context;
    private readonly GetDrugAlternativesHandler _handler;

    public GetDrugAlternativesHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new GetDrugAlternativesHandler(_context);
    }

    private DrugCatalogEntry SeedDrug(
        string innName,
        string displayName,
        DrugCategory category = DrugCategory.Antibiotic,
        bool isActive = true,
        Guid? clinicId = null)
    {
        var result = DrugCatalogEntry.Create(innName, displayName, category, clinicId);
        var entry = result.Value;
        if (!isActive)
            entry.Deactivate();
        _context.DrugCatalogEntries.Add(entry);
        _context.SaveChanges();
        return entry;
    }

    private void SeedPrescriptionForPatient(Guid drugCatalogEntryId)
    {
        // Create a medical record for the patient, then a prescription linked to it
        var mr = MedicalRecord.Create(ClinicId, PatientId, "Checkup", "Treatment", "Dr. Smith", DateTime.UtcNow);
        _context.MedicalRecords.Add(mr.Value);
        _context.SaveChanges();

        var prescription = Prescription.Create(
            ClinicId,
            mr.Value.Id,
            "Test Med",
            "10mg daily",
            "VET-001",
            drugCatalogEntryId);
        _context.Prescriptions.Add(prescription.Value);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenDrugNotFound_ReturnsNotFound()
    {
        var query = new GetDrugAlternativesQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDrugIsInactive_ReturnsNotFound()
    {
        var drug = SeedDrug("Amoxicillin", "Amoxicillin 250mg", isActive: false);
        var query = new GetDrugAlternativesQuery(drug.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoAlternativesExist_ReturnsEmptyList()
    {
        var drug = SeedDrug("Amoxicillin", "Amoxicillin 250mg");
        var query = new GetDrugAlternativesQuery(drug.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsSameCategoryAlternatives()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        SeedDrug("Doxycycline", "Doxycycline 100mg", DrugCategory.Antibiotic);
        SeedDrug("Meloxicam", "Metacam 5mg/ml", DrugCategory.AntiInflammatory);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ActiveIngredient.Should().Be("Doxycycline");
    }

    [Fact]
    public async Task Handle_SameActiveIngredientRankedFirst()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        SeedDrug("Doxycycline", "Doxycycline 100mg", DrugCategory.Antibiotic);
        SeedDrug("Amoxicillin", "Clamoxyl 500mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        // Same active ingredient should come first
        result.Value[0].ActiveIngredient.Should().Be("Amoxicillin");
        result.Value[0].Name.Should().Be("Clamoxyl 500mg");
        result.Value[1].ActiveIngredient.Should().Be("Doxycycline");
    }

    [Fact]
    public async Task Handle_ExcludesSourceDrug()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotContain(a => a.DrugId == source.Id);
    }

    [Fact]
    public async Task Handle_ExcludesInactiveAlternatives()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        SeedDrug("Doxycycline", "Doxycycline 100mg", DrugCategory.Antibiotic, isActive: false);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_LimitsTo10Results()
    {
        var source = SeedDrug("SourceDrug", "SourceDrug 100mg", DrugCategory.Antibiotic);
        for (int i = 0; i < 15; i++)
            SeedDrug($"Alt{i:D2}", $"Alt{i:D2} 100mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_WithPatientId_ExcludesDrugsWithInteractions()
    {
        // Create drugs with interactions before first SaveChanges
        var sourceResult = DrugCatalogEntry.Create("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        var currentDrugResult = DrugCatalogEntry.Create("Metronidazole", "Metronidazole 500mg", DrugCategory.Antibiotic);
        var interactingAltResult = DrugCatalogEntry.Create("Doxycycline", "Doxycycline 100mg", DrugCategory.Antibiotic);
        var safeAltResult = DrugCatalogEntry.Create("Enrofloxacin", "Baytril 50mg", DrugCategory.Antibiotic);

        var source = sourceResult.Value;
        var currentDrug = currentDrugResult.Value;
        var interactingAlt = interactingAltResult.Value;
        var safeAlt = safeAltResult.Value;

        // Add interaction before persisting
        currentDrug.AddInteraction(interactingAlt.Id, "Doxycycline", InteractionSeverity.Moderate, "Reduces efficacy");

        _context.DrugCatalogEntries.AddRange(source, currentDrug, interactingAlt, safeAlt);
        _context.SaveChanges();

        // Patient is currently on currentDrug
        SeedPrescriptionForPatient(currentDrug.Id);

        var query = new GetDrugAlternativesQuery(source.Id, PatientId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotContain(a => a.DrugId == interactingAlt.Id);
        result.Value.Should().Contain(a => a.DrugId == safeAlt.Id);
    }

    [Fact]
    public async Task Handle_WithoutPatientId_IncludesAllAlternatives()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        var alt = SeedDrug("Doxycycline", "Doxycycline 100mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].DrugId.Should().Be(alt.Id);
    }

    [Fact]
    public async Task Handle_GenericDrugHasIsGenericTrue()
    {
        var source = SeedDrug("Amoxicillin", "Clamoxyl 500mg", DrugCategory.Antibiotic);
        SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].IsGeneric.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BrandedDrugHasIsGenericFalse()
    {
        var source = SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);
        SeedDrug("Amoxicillin", "Clamoxyl 500mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].IsGeneric.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_GenericAlternativeHasLowerPriceIndicator()
    {
        var source = SeedDrug("Amoxicillin", "Clamoxyl 500mg", DrugCategory.Antibiotic);
        SeedDrug("Amoxicillin", "Amoxicillin 250mg", DrugCategory.Antibiotic);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].PriceIndicator.Should().Be(PriceIndicator.Lower);
    }

    [Fact]
    public async Task Handle_DifferentCategoryDrugWithSameInnIncluded()
    {
        // A drug might be categorized differently but share the same INN
        var source = SeedDrug("Prednisolone", "Prednisolone 5mg", DrugCategory.AntiInflammatory);
        SeedDrug("Prednisolone", "Prednisolone Injectable 10mg/ml", DrugCategory.Hormone);

        var query = new GetDrugAlternativesQuery(source.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ActiveIngredient.Should().Be("Prednisolone");
    }

    [Fact]
    public void DeriveFormulationType_Injectable()
    {
        GetDrugAlternativesHandler.DeriveFormulationType("Metacam 5mg/ml").Should().Be("Injectable");
        GetDrugAlternativesHandler.DeriveFormulationType("Drug Injectable").Should().Be("Injectable");
    }

    [Fact]
    public void DeriveFormulationType_Tablet()
    {
        GetDrugAlternativesHandler.DeriveFormulationType("Amoxicillin 250mg").Should().Be("Tablet");
    }

    [Fact]
    public void DeriveFormulationType_OralLiquid()
    {
        GetDrugAlternativesHandler.DeriveFormulationType("Amoxicillin Oral Suspension").Should().Be("Oral Liquid");
    }

    [Fact]
    public void DeriveFormulationType_Topical()
    {
        GetDrugAlternativesHandler.DeriveFormulationType("Cortisone Cream").Should().Be("Topical");
    }

    [Fact]
    public void DeriveFormulationType_Unknown_ReturnsOther()
    {
        GetDrugAlternativesHandler.DeriveFormulationType("SomeDrug").Should().Be("Other");
    }

    public void Dispose() => _context.Dispose();
}
