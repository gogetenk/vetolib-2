using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalPrescriptions;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalRecords;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalVaccinations;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalWeightHistory;
using Vetolib.MedicalRecords.Application.Queries.Portal.GetMyAnimals;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class OwnerPortalHandlerTests
{
    private static readonly Guid OwnerAccountId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public async Task GetMyAnimals_WithEmptyOwnerAccountId_ReturnsInvalid()
    {
        var context = CreateInMemoryContext();
        var handler = new GetMyAnimalsHandler(context);

        var result = await handler.Handle(new GetMyAnimalsQuery(Guid.Empty), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task GetAnimalRecords_WhenOwnerNotLinked_ReturnsForbidden()
    {
        var ownerAuth = Substitute.For<IOwnerAuthorizationService>();
        ownerAuth.IsOwnerLinkedToPatient(OwnerAccountId, PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));

        var context = CreateInMemoryContext();
        var handler = new GetAnimalRecordsHandler(ownerAuth, context);

        var result = await handler.Handle(new GetAnimalRecordsQuery(OwnerAccountId, PatientId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task GetAnimalVaccinations_WhenOwnerNotLinked_ReturnsForbidden()
    {
        var ownerAuth = Substitute.For<IOwnerAuthorizationService>();
        ownerAuth.IsOwnerLinkedToPatient(OwnerAccountId, PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));

        var context = CreateInMemoryContext();
        var handler = new GetAnimalVaccinationsHandler(ownerAuth, context);

        var result = await handler.Handle(new GetAnimalVaccinationsQuery(OwnerAccountId, PatientId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task GetAnimalPrescriptions_WhenOwnerNotLinked_ReturnsForbidden()
    {
        var ownerAuth = Substitute.For<IOwnerAuthorizationService>();
        ownerAuth.IsOwnerLinkedToPatient(OwnerAccountId, PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));

        var context = CreateInMemoryContext();
        var handler = new GetAnimalPrescriptionsHandler(ownerAuth, context);

        var result = await handler.Handle(new GetAnimalPrescriptionsQuery(OwnerAccountId, PatientId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task GetAnimalWeightHistory_WhenOwnerNotLinked_ReturnsForbidden()
    {
        var ownerAuth = Substitute.For<IOwnerAuthorizationService>();
        ownerAuth.IsOwnerLinkedToPatient(OwnerAccountId, PatientId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));

        var context = CreateInMemoryContext();
        var handler = new GetAnimalWeightHistoryHandler(ownerAuth, context);

        var result = await handler.Handle(new GetAnimalWeightHistoryQuery(OwnerAccountId, PatientId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task GetAnimalRecords_WhenAuthReturnsInvalid_ReturnsInvalid()
    {
        var ownerAuth = Substitute.For<IOwnerAuthorizationService>();
        ownerAuth.IsOwnerLinkedToPatient(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Invalid(new ValidationError("ownerAccountId", "OwnerAccountId is required")));

        var context = CreateInMemoryContext();
        var handler = new GetAnimalRecordsHandler(ownerAuth, context);

        var result = await handler.Handle(new GetAnimalRecordsQuery(Guid.Empty, PatientId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    private static MedicalRecordsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(Guid.NewGuid());

        var publisher = Substitute.For<IPublisher>();

        return new MedicalRecordsDbContext(options, clinicContext, publisher);
    }
}
