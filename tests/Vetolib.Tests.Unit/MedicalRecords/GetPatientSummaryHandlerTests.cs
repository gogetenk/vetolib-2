using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.GetPatientSummary;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class GetPatientSummaryHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly GetPatientSummaryHandler _handler;

    public GetPatientSummaryHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new GetPatientSummaryHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFound()
    {
        var query = new GetPatientSummaryQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPatientExists_ReturnsPatientInfo()
    {
        var patient = SeedPatient();

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patient.Id.Should().Be(patient.Id);
        result.Value.Patient.Name.Should().Be("Bella");
        result.Value.Patient.Species.Should().Be(Species.Dog);
        result.Value.Patient.Breed.Should().Be("Labrador");
        result.Value.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WhenNoRecords_ReturnsEmptyCollections()
    {
        var patient = SeedPatient();

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecentMedicalRecords.Should().BeEmpty();
        result.Value.ActivePrescriptions.Should().BeEmpty();
        result.Value.Vaccinations.Should().BeEmpty();
        result.Value.HealthAlerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsLastTenMedicalRecords()
    {
        var patient = SeedPatient();

        // Seed 12 records
        for (int i = 0; i < 12; i++)
        {
            SeedMedicalRecord(patient.Id, $"Diagnosis {i}", DateTime.UtcNow.AddDays(-i));
        }

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecentMedicalRecords.Should().HaveCount(10);
        // Most recent first
        result.Value.RecentMedicalRecords[0].Diagnosis.Should().Be("Diagnosis 0");
    }

    [Fact]
    public async Task Handle_ReturnsActivePrescriptionsFromLast90Days()
    {
        var patient = SeedPatient();

        // Recent record with prescription (within 90 days)
        var recentRecord = SeedMedicalRecord(patient.Id, "Recent checkup", DateTime.UtcNow.AddDays(-10));
        SeedPrescription(recentRecord.Id, "Amoxicillin", "250mg twice daily");

        // Old record with prescription (older than 90 days)
        var oldRecord = SeedMedicalRecord(patient.Id, "Old checkup", DateTime.UtcNow.AddDays(-100));
        SeedPrescription(oldRecord.Id, "Expired drug", "100mg");

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActivePrescriptions.Should().HaveCount(1);
        result.Value.ActivePrescriptions[0].Medication.Should().Be("Amoxicillin");
    }

    [Fact]
    public async Task Handle_ReturnsVaccinations()
    {
        var patient = SeedPatient();

        SeedMedicalRecord(patient.Id, "Annual vaccination", DateTime.UtcNow.AddDays(-30), treatment: "VACCINE: Rabies");
        SeedMedicalRecord(patient.Id, "Puppy shots", DateTime.UtcNow.AddDays(-365), treatment: "VACCINE: DHPP");

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Vaccinations.Should().HaveCount(2);
        result.Value.Vaccinations[0].Name.Should().Be("Rabies");
    }

    [Fact]
    public async Task Handle_ReturnsHealthAlerts()
    {
        var patient = SeedPatient();

        SeedMedicalRecord(patient.Id, "ALLERGY: Penicillin", DateTime.UtcNow.AddDays(-60));
        SeedMedicalRecord(patient.Id, "ALLERGY: Latex", DateTime.UtcNow.AddDays(-30));
        SeedMedicalRecord(patient.Id, "Normal checkup", DateTime.UtcNow.AddDays(-5));

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.HealthAlerts.Should().HaveCount(2);
        result.Value.HealthAlerts.Should().Contain("Penicillin");
        result.Value.HealthAlerts.Should().Contain("Latex");
    }

    [Fact]
    public async Task Handle_WhenOwnerExists_ReturnsOwnerInfo()
    {
        var patient = SeedPatientWithOwner();

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Owner.Should().NotBeNull();
        result.Value.Owner!.FullName.Should().Be("Ahmed Al-Rashid");
        result.Value.Owner.Email.Should().Be("ahmed@email.ae");
    }

    [Fact]
    public async Task Handle_WhenNoOwner_ReturnsNullOwner()
    {
        var patient = SeedPatient();

        var query = new GetPatientSummaryQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Owner.Should().BeNull();
    }

    private Patient SeedPatient()
    {
        var patientResult = Patient.Create(ClinicId, "Bella", Species.Dog, "Labrador", new DateOnly(2020, 5, 10));
        var patient = patientResult.Value;
        _context.Patients.Add(patient);
        _context.SaveChanges();
        return patient;
    }

    private Patient SeedPatientWithOwner()
    {
        var patient = SeedPatient();
        var ownerResult = Owner.Create(ClinicId, "Ahmed", "Al-Rashid", "ahmed@email.ae", "+971 50 123 4567");
        var owner = ownerResult.Value;
        _context.Owners.Add(owner);
        _context.SaveChanges();

        var patientOwner = PatientOwner.Create(ClinicId, patient.Id, owner.Id);
        _context.PatientOwners.Add(patientOwner);
        _context.SaveChanges();

        // Reload patient with owners
        _context.Entry(patient).Collection(p => p.PatientOwners).Load();
        foreach (var po in patient.PatientOwners)
        {
            _context.Entry(po).Reference(p => p.Owner).Load();
        }

        return patient;
    }

    private MedicalRecord SeedMedicalRecord(Guid patientId, string diagnosis, DateTime? examinedAt = null, string treatment = "Standard care")
    {
        var recordResult = MedicalRecord.Create(
            ClinicId,
            patientId,
            diagnosis,
            treatment,
            "Dr. Al-Rashidi",
            examinedAt ?? DateTime.UtcNow);

        var record = recordResult.Value;
        _context.MedicalRecords.Add(record);
        _context.SaveChanges();
        return record;
    }

    private void SeedPrescription(Guid medicalRecordId, string medication, string dosage)
    {
        var prescriptionResult = Prescription.Create(
            ClinicId,
            medicalRecordId,
            medication,
            dosage,
            "LIC-12345");

        _context.Prescriptions.Add(prescriptionResult.Value);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();
}
