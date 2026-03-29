using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Breeding.Application.Commands.CreateLitter;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Breeding;

public class CreateLitterHandlerTests
{
    private readonly Guid ClinicId = Guid.NewGuid();
    private readonly Guid MotherId = Guid.NewGuid();
    private readonly Guid FatherId = Guid.NewGuid();
    private readonly DateOnly ValidDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

    private readonly IPatientReader _patientReader = Substitute.For<IPatientReader>();

    private BreedingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;

        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);
        var publisher = Substitute.For<IPublisher>();

        return new BreedingDbContext(options, clinicContext, publisher);
    }

    [Fact]
    public async Task Handle_MotherNotFound_ReturnsNotFound()
    {
        _patientReader.GetPatientBasicInfoAsync(MotherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.NotFound());

        using var context = CreateInMemoryContext();
        var handler = new CreateLitterHandler(context, _patientReader);

        var result = await handler.Handle(
            new CreateLitterCommand(ClinicId, MotherId, null, null, ValidDate, 1, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_MotherIsMale_ReturnsError()
    {
        _patientReader.GetPatientBasicInfoAsync(MotherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(MotherId, "Buraq", Species.Horse, Sex.Male)));

        using var context = CreateInMemoryContext();
        var handler = new CreateLitterHandler(context, _patientReader);

        var result = await handler.Handle(
            new CreateLitterCommand(ClinicId, MotherId, null, null, ValidDate, 1, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("female"));
    }

    [Fact]
    public async Task Handle_FatherDifferentSpecies_ReturnsError()
    {
        _patientReader.GetPatientBasicInfoAsync(MotherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(MotherId, "Shams", Species.Horse, Sex.Female)));

        _patientReader.GetPatientBasicInfoAsync(FatherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(FatherId, "Felix", Species.Cat, Sex.Male)));

        using var context = CreateInMemoryContext();
        var handler = new CreateLitterHandler(context, _patientReader);

        var result = await handler.Handle(
            new CreateLitterCommand(ClinicId, MotherId, FatherId, null, ValidDate, 1, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("same species"));
    }

    [Fact]
    public async Task Handle_ValidInput_CreatesLitter()
    {
        _patientReader.GetPatientBasicInfoAsync(MotherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(MotherId, "Shams", Species.Horse, Sex.Female)));

        _patientReader.GetPatientBasicInfoAsync(FatherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(FatherId, "Buraq", Species.Horse, Sex.Male)));

        using var context = CreateInMemoryContext();
        var handler = new CreateLitterHandler(context, _patientReader);

        var result = await handler.Handle(
            new CreateLitterCommand(ClinicId, MotherId, FatherId, null, ValidDate, 1, 1, "Healthy"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MotherPatientId.Should().Be(MotherId);
        result.Value.FatherPatientId.Should().Be(FatherId);

        var saved = await context.Litters.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_FatherNotFound_ReturnsNotFound()
    {
        _patientReader.GetPatientBasicInfoAsync(MotherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.Success(
                new PatientBasicInfoDto(MotherId, "Shams", Species.Horse, Sex.Female)));

        _patientReader.GetPatientBasicInfoAsync(FatherId, Arg.Any<CancellationToken>())
            .Returns(Result<PatientBasicInfoDto>.NotFound());

        using var context = CreateInMemoryContext();
        var handler = new CreateLitterHandler(context, _patientReader);

        var result = await handler.Handle(
            new CreateLitterCommand(ClinicId, MotherId, FatherId, null, ValidDate, 1, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
