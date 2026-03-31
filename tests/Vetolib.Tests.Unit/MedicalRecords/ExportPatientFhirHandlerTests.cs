using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class ExportPatientFhirHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly ExportPatientFhirHandler _handler;

    public ExportPatientFhirHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new ExportPatientFhirHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFound()
    {
        var query = new ExportPatientFhirQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPatientExists_ReturnsFhirBundle()
    {
        var patient = SeedPatient();

        var query = new ExportPatientFhirQuery(patient.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/fhir+json");

        var doc = JsonDocument.Parse(result.Value.Json);
        doc.RootElement.GetProperty("resourceType").GetString().Should().Be("Bundle");
    }

    [Fact]
    public async Task Handle_IncludesPatientResource()
    {
        var patient = SeedPatient();

        var result = await _handler.Handle(new ExportPatientFhirQuery(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var doc = JsonDocument.Parse(result.Value.Json);
        var entries = doc.RootElement.GetProperty("entry");
        entries[0].GetProperty("resource").GetProperty("resourceType").GetString().Should().Be("Patient");
        entries[0].GetProperty("resource").GetProperty("id").GetString().Should().Be(patient.Id.ToString());
    }

    [Fact]
    public async Task Handle_WithOwner_IncludesRelatedPerson()
    {
        var patient = SeedPatientWithOwner();

        var result = await _handler.Handle(new ExportPatientFhirQuery(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var doc = JsonDocument.Parse(result.Value.Json);
        var entries = doc.RootElement.GetProperty("entry");

        var resourceTypes = Enumerable.Range(0, entries.GetArrayLength())
            .Select(i => entries[i].GetProperty("resource").GetProperty("resourceType").GetString())
            .ToList();

        resourceTypes.Should().Contain("RelatedPerson");
    }

    [Fact]
    public async Task Handle_WithMedicalRecords_IncludesEncounters()
    {
        var patient = SeedPatient();
        SeedMedicalRecord(patient.Id, "Checkup");

        var result = await _handler.Handle(new ExportPatientFhirQuery(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var doc = JsonDocument.Parse(result.Value.Json);
        var entries = doc.RootElement.GetProperty("entry");

        var resourceTypes = Enumerable.Range(0, entries.GetArrayLength())
            .Select(i => entries[i].GetProperty("resource").GetProperty("resourceType").GetString())
            .ToList();

        resourceTypes.Should().Contain("Encounter");
    }

    [Fact]
    public async Task Handle_WithWeightEntries_IncludesObservations()
    {
        var patient = SeedPatient();
        SeedWeightEntry(patient.Id, 25.5m);

        var result = await _handler.Handle(new ExportPatientFhirQuery(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var doc = JsonDocument.Parse(result.Value.Json);
        var entries = doc.RootElement.GetProperty("entry");

        var resourceTypes = Enumerable.Range(0, entries.GetArrayLength())
            .Select(i => entries[i].GetProperty("resource").GetProperty("resourceType").GetString())
            .ToList();

        resourceTypes.Should().Contain("Observation");
    }

    [Fact]
    public async Task Handle_WithPrescriptions_IncludesMedicationRequests()
    {
        var patient = SeedPatient();
        var record = SeedMedicalRecord(patient.Id, "Infection");
        SeedPrescription(record.Id, "Amoxicillin");

        var result = await _handler.Handle(new ExportPatientFhirQuery(patient.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var doc = JsonDocument.Parse(result.Value.Json);
        var entries = doc.RootElement.GetProperty("entry");

        var resourceTypes = Enumerable.Range(0, entries.GetArrayLength())
            .Select(i => entries[i].GetProperty("resource").GetProperty("resourceType").GetString())
            .ToList();

        resourceTypes.Should().Contain("MedicationRequest");
    }

    // --- Seed helpers ---

    private Patient SeedPatient()
    {
        var patientResult = Patient.Create(ClinicId, "Bella", Species.Dog, "Labrador",
            new DateOnly(2020, 5, 10), Sex.Female, "123456789012345");
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

        _context.Entry(patient).Collection(p => p.PatientOwners).Load();
        foreach (var po in patient.PatientOwners)
            _context.Entry(po).Reference(p => p.Owner).Load();

        return patient;
    }

    private MedicalRecord SeedMedicalRecord(Guid patientId, string diagnosis)
    {
        var recordResult = MedicalRecord.Create(ClinicId, patientId, diagnosis, "Treatment",
            "Dr. Al-Rashidi", DateTime.UtcNow.AddDays(-5));
        var record = recordResult.Value;
        _context.MedicalRecords.Add(record);
        _context.SaveChanges();
        return record;
    }

    private void SeedPrescription(Guid medicalRecordId, string medication)
    {
        var prescriptionResult = Prescription.Create(ClinicId, medicalRecordId, medication, "250mg", "LIC-12345");
        _context.Prescriptions.Add(prescriptionResult.Value);
        _context.SaveChanges();
    }

    private void SeedWeightEntry(Guid patientId, decimal weightKg)
    {
        var weightResult = WeightEntry.Create(ClinicId, patientId, weightKg, "Dr. Al-Rashidi");
        _context.WeightEntries.Add(weightResult.Value);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();
}
