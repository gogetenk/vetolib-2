using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.UpdatePatient;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class UpdatePatientHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly UpdatePatientHandler _handler;

    private Guid PatientId { get; set; }

    public UpdatePatientHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new UpdatePatientHandler(_context);

        SeedPatient();
    }

    private void SeedPatient()
    {
        var ownerResult = Owner.Create(ClinicId, "Faisal", "Al-Kuwari", "faisal@vetoclinic.ae", "+971501234567");
        var owner = ownerResult.Value;
        _context.Owners.Add(owner);

        var patientResult = Patient.Create(ClinicId, "Bella", Species.Dog, "Labrador", new DateOnly(2020, 5, 10));
        var patient = patientResult.Value;

        var patientOwner = PatientOwner.Create(ClinicId, patient.Id, owner.Id);
        patient.AddOwner(patientOwner);

        _context.Patients.Add(patient);
        _context.SaveChanges();

        PatientId = patient.Id;
    }

    [Fact]
    public async Task Handle_HappyPath_UpdatesPatientName()
    {
        var cmd = new UpdatePatientCommand(
            PatientId: PatientId,
            Name: "Bella Updated",
            Species: null,
            Breed: null,
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bella Updated");
    }

    [Fact]
    public async Task Handle_HappyPath_UpdatesOwnerPhone()
    {
        var cmd = new UpdatePatientCommand(
            PatientId: PatientId,
            Name: null,
            Species: null,
            Breed: null,
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: "+971509876543");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OwnerPhone.Should().Be("+971509876543");
    }

    [Fact]
    public async Task Handle_HappyPath_UpdatesSpecies()
    {
        var cmd = new UpdatePatientCommand(
            PatientId: PatientId,
            Name: null,
            Species: Species.Cat,
            Breed: null,
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Species.Should().Be(Species.Cat);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFound()
    {
        var cmd = new UpdatePatientCommand(
            PatientId: Guid.NewGuid(), // does not exist
            Name: "Ghost",
            Species: null,
            Breed: null,
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNameIsEmptyString_ReturnsError()
    {
        // Passing an empty string triggers the domain guard in Patient.UpdateInfo
        var cmd = new UpdatePatientCommand(
            PatientId: PatientId,
            Name: "",
            Species: null,
            Breed: null,
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_WhenBreedIsEmptyString_ReturnsError()
    {
        var cmd = new UpdatePatientCommand(
            PatientId: PatientId,
            Name: null,
            Species: null,
            Breed: "",
            BirthDate: null,
            OwnerName: null,
            OwnerPhone: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    public void Dispose() => _context.Dispose();
}
