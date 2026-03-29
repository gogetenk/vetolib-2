using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Breeding.Application.Commands.RecordHeatCycle;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class RecordHeatCycleHandlerTests : IDisposable
{
    private readonly BreedingDbContext _context;
    private readonly IPatientReader _patientReader;
    private readonly RecordHeatCycleHandler _handler;
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    public RecordHeatCycleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);
        var publisher = Substitute.For<IPublisher>();

        _context = new BreedingDbContext(options, clinicContext, publisher);
        _patientReader = Substitute.For<IPatientReader>();
        _handler = new RecordHeatCycleHandler(_context, _patientReader);
    }

    [Fact]
    public async Task Handle_MalePatient_ShouldRejectWithError()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(new PatientBasicInfoDto(PatientId, "Test", Species.Dog, Sex.Male)));

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10));
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Only female patients"));
    }

    [Fact]
    public async Task Handle_NeuteredMalePatient_ShouldRejectWithError()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(new PatientBasicInfoDto(PatientId, "Test", Species.Dog, Sex.NeuteredMale)));

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10));
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Only female patients"));
    }

    [Fact]
    public async Task Handle_SpayedFemalePatient_ShouldRejectWithError()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(new PatientBasicInfoDto(PatientId, "Test", Species.Dog, Sex.SpayedFemale)));

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10));
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Spayed patients"));
    }

    [Fact]
    public async Task Handle_FemalePatient_ShouldSucceed()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(new PatientBasicInfoDto(PatientId, "Test", Species.Dog, Sex.Female)));

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 25));
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.StartDate.Should().Be(new DateOnly(2026, 1, 10));
    }

    [Fact]
    public async Task Handle_PatientNotFound_ShouldReturnNotFound()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.NotFound());

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10));
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WithNotes_ShouldRecordNotes()
    {
        _patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(new PatientBasicInfoDto(PatientId, "Test", Species.Dog, Sex.Female)));

        var cmd = new RecordHeatCycleCommand(ClinicId, PatientId, new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 25), "Strong signs");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be("Strong signs");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
