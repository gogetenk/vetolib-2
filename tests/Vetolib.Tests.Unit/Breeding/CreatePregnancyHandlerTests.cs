using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Breeding.Application.Commands.CreatePregnancy;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class CreatePregnancyHandlerTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");

    private static (CreatePregnancyHandler handler, BreedingDbContext context) CreateHandler(Sex sex = Sex.Female, Species species = Species.Dog)
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var patientReader = Substitute.For<IPatientReader>();
        patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(PatientId, "TestPatient", species, sex)));

        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;

        var context = new BreedingDbContext(options, clinicContext, publisher);
        var handler = new CreatePregnancyHandler(context, clinicContext, patientReader);

        return (handler, context);
    }

    [Fact]
    public async Task Handle_MalePatient_ReturnsError()
    {
        var (handler, _) = CreateHandler(sex: Sex.Male);
        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Only female patients"));
    }

    [Fact]
    public async Task Handle_NeuteredMalePatient_ReturnsError()
    {
        var (handler, _) = CreateHandler(sex: Sex.NeuteredMale);
        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Only female patients"));
    }

    [Fact]
    public async Task Handle_SpayedFemalePatient_ReturnsError()
    {
        var (handler, _) = CreateHandler(sex: Sex.SpayedFemale);
        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Spayed patients"));
    }

    [Fact]
    public async Task Handle_ValidFemale_CreatesPregnancy()
    {
        var (handler, context) = CreateHandler(sex: Sex.Female, species: Species.Dog);
        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, "Healthy mare");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.MatingMethod.Should().Be(MatingMethod.Natural);
        result.Value.Status.Should().Be(PregnancyStatus.Active);

        var saved = await context.Pregnancies.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_OverlappingActivePregnancy_ReturnsError()
    {
        var (handler, _) = CreateHandler(sex: Sex.Female, species: Species.Dog);
        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, null);

        // First pregnancy succeeds
        var first = await handler.Handle(cmd, CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        // Second pregnancy for same patient fails
        var second = await handler.Handle(cmd, CancellationToken.None);
        second.IsSuccess.Should().BeFalse();
        second.Errors.Should().Contain(e => e.Contains("already has an active pregnancy"));
    }

    [Fact]
    public async Task Handle_PatientNotFound_ReturnsNotFound()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);
        var publisher = Substitute.For<IPublisher>();
        var patientReader = Substitute.For<IPatientReader>();
        patientReader.GetPatientBasicInfoAsync(PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.NotFound());

        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        var context = new BreedingDbContext(options, clinicContext, publisher);
        var handler = new CreatePregnancyHandler(context, clinicContext, patientReader);

        var cmd = new CreatePregnancyCommand(
            PatientId, null, new DateOnly(2026, 2, 15), MatingMethod.Natural, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
