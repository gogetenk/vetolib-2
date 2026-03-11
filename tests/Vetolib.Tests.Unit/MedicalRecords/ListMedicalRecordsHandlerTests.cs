using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class ListMedicalRecordsHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly ListMedicalRecordsHandler _handler;

    private Guid PatientId { get; set; }

    public ListMedicalRecordsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new ListMedicalRecordsHandler(_context);

        SeedPatient();
    }

    private void SeedPatient()
    {
        var patientResult = Patient.Create(ClinicId, "Bella", Species.Dog, "Labrador", new DateOnly(2020, 5, 10));
        var patient = patientResult.Value;
        _context.Patients.Add(patient);
        _context.SaveChanges();
        PatientId = patient.Id;
    }

    private void SeedMedicalRecord(Guid patientId, string diagnosis = "Routine checkup", DateTime? examinedAt = null)
    {
        var recordResult = MedicalRecord.Create(
            ClinicId,
            patientId,
            diagnosis,
            "Standard care",
            "Dr. Al-Rashidi",
            examinedAt ?? DateTime.UtcNow);

        _context.MedicalRecords.Add(recordResult.Value);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenNoRecords_ReturnsEmptyPagedResult()
    {
        var query = new ListMedicalRecordsQuery(PatientId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WhenRecordsExist_ReturnsPagedResultWithItems()
    {
        SeedMedicalRecord(PatientId, "Ear infection");
        SeedMedicalRecord(PatientId, "Dental cleaning");
        var query = new ListMedicalRecordsQuery(PatientId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenMultiplePages_ReturnsCorrectPage()
    {
        for (int i = 0; i < 5; i++)
            SeedMedicalRecord(PatientId, $"Diagnosis {i}");

        var query = new ListMedicalRecordsQuery(PatientId, Page: 2, PageSize: 2);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(5);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Handle_RecordsOrderedByExaminedAtDescending()
    {
        var older = DateTime.UtcNow.AddDays(-10);
        var newer = DateTime.UtcNow.AddDays(-1);

        SeedMedicalRecord(PatientId, "Older record", older);
        SeedMedicalRecord(PatientId, "Newer record", newer);

        var query = new ListMedicalRecordsQuery(PatientId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Diagnosis.Should().Be("Newer record");
        result.Value.Items[1].Diagnosis.Should().Be("Older record");
    }

    [Fact]
    public async Task Handle_WhenRecordsBelongToAnotherPatient_DoesNotReturnThem()
    {
        // Seed records for a different patient — not seeded into the DB, just a random Guid
        var otherPatientId = Guid.NewGuid();

        // We seed a record using the correct ClinicId but a non-existent patient (FK not enforced in InMemory)
        var recordResult = MedicalRecord.Create(
            ClinicId,
            otherPatientId,
            "Other patient visit",
            "Treatment",
            "Dr. Hassan",
            DateTime.UtcNow);
        _context.MedicalRecords.Add(recordResult.Value);
        _context.SaveChanges();

        SeedMedicalRecord(PatientId, "My patient visit");

        var query = new ListMedicalRecordsQuery(PatientId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Diagnosis.Should().Be("My patient visit");
    }

    [Fact]
    public async Task Handle_PageSizeClampedAt100()
    {
        for (int i = 0; i < 5; i++)
            SeedMedicalRecord(PatientId, $"Diagnosis {i}");

        var query = new ListMedicalRecordsQuery(PatientId, Page: 1, PageSize: 999);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(100);
        result.Value.Items.Should().HaveCount(5);
    }

    public void Dispose() => _context.Dispose();
}
