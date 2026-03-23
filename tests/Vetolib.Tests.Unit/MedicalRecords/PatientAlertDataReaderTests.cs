using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class PatientAlertDataReaderTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly PatientAlertDataReader _reader;

    public PatientAlertDataReaderTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _reader = new PatientAlertDataReader(_context);
    }

    [Fact]
    public async Task GetAllActivePatientsWithRecords_ReturnsSuccess_WithCorrectMapping()
    {
        // Arrange
        var patient = Patient.Create(ClinicId, "Bella", Species.Dog, "Labrador", new DateOnly(2020, 5, 10)).Value;
        patient.SetWeight(25.5m);
        _context.Patients.Add(patient);

        var record = MedicalRecord.Create(
            ClinicId, patient.Id, "Annual checkup", "Vaccination", "Dr. Smith", DateTime.UtcNow.AddDays(-10)).Value;
        _context.MedicalRecords.Add(record);

        await _context.SaveChangesAsync();

        // Act
        var result = await _reader.GetAllActivePatientsWithRecordsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value[0];
        dto.PatientId.Should().Be(patient.Id);
        dto.Name.Should().Be("Bella");
        dto.Species.Should().Be(Species.Dog);
        dto.Breed.Should().Be("Labrador");
        dto.BirthDate.Should().Be(new DateOnly(2020, 5, 10));
        dto.WeightKg.Should().Be(25.5m);
        dto.RecentRecords.Should().HaveCount(1);
        dto.RecentRecords[0].Diagnosis.Should().Be("Annual checkup");
        dto.WeightHistory.Should().HaveCount(1);
        dto.WeightHistory[0].WeightKg.Should().Be(25.5m);
    }

    [Fact]
    public async Task GetAllActivePatientsWithRecords_PatientWithNoWeight_ReturnsEmptyWeightHistory()
    {
        // Arrange
        var patient = Patient.Create(ClinicId, "Max", Species.Cat, "Persian", new DateOnly(2019, 3, 1)).Value;
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reader.GetAllActivePatientsWithRecordsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.WeightKg.Should().BeNull();
        dto.WeightHistory.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivePatientsWithRecords_OldRecords_AreExcluded()
    {
        // Arrange
        var patient = Patient.Create(ClinicId, "Rocky", Species.Dog, "Husky", new DateOnly(2018, 1, 1)).Value;
        _context.Patients.Add(patient);

        // Record older than 24 months
        var oldRecord = MedicalRecord.Create(
            ClinicId, patient.Id, "Old diagnosis", "Old treatment", "Dr. Old", DateTime.UtcNow.AddMonths(-30)).Value;
        _context.MedicalRecords.Add(oldRecord);

        // Recent record
        var recentRecord = MedicalRecord.Create(
            ClinicId, patient.Id, "Recent diagnosis", "Recent treatment", "Dr. New", DateTime.UtcNow.AddDays(-5)).Value;
        _context.MedicalRecords.Add(recentRecord);

        await _context.SaveChangesAsync();

        // Act
        var result = await _reader.GetAllActivePatientsWithRecordsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value[0];
        dto.RecentRecords.Should().HaveCount(1);
        dto.RecentRecords[0].Diagnosis.Should().Be("Recent diagnosis");
    }

    [Fact]
    public async Task GetAllActivePatientsWithRecords_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _reader.GetAllActivePatientsWithRecordsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
