using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddMedicalRecordHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly AddMedicalRecordHandler _handler;

    private Guid PatientId { get; set; }

    public AddMedicalRecordHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new AddMedicalRecordHandler(_context);

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

    private AddMedicalRecordCommand BuildCommand(
        Guid? patientId = null,
        string diagnosis = "Annual wellness checkup",
        string treatment = "Supportive care, no medication required",
        string vetName = "Dr. Al-Rashidi")
        => new(
            ClinicId: ClinicId,
            PatientId: patientId ?? PatientId,
            Diagnosis: diagnosis,
            Treatment: treatment,
            VetName: vetName);

    [Fact]
    public async Task Handle_HappyPath_CreatesMedicalRecordForPatient()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.Diagnosis.Should().Be("Annual wellness checkup");
        result.Value.VetName.Should().Be("Dr. Al-Rashidi");
    }

    [Fact]
    public async Task Handle_HappyPath_PersistsMedicalRecordToDatabase()
    {
        var cmd = BuildCommand(diagnosis: "Respiratory infection");

        await _handler.Handle(cmd, CancellationToken.None);

        var count = await _context.MedicalRecords.CountAsync();
        count.Should().Be(1);
        var saved = await _context.MedicalRecords.FirstAsync();
        saved.Diagnosis.Should().Be("Respiratory infection");
    }

    [Fact]
    public async Task Handle_HappyPath_SetsExaminedAtTimestamp()
    {
        var before = DateTime.UtcNow;
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExaminedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFound()
    {
        var cmd = BuildCommand(patientId: Guid.NewGuid()); // does not exist

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDiagnosisIsEmpty_ReturnsInvalid()
    {
        var cmd = BuildCommand(diagnosis: "");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenVetNameIsEmpty_ReturnsInvalid()
    {
        var cmd = BuildCommand(vetName: "");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenTreatmentIsEmpty_ReturnsInvalid()
    {
        var cmd = BuildCommand(treatment: "");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose() => _context.Dispose();
}
