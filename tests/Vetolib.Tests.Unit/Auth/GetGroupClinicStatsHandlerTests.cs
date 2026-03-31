using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.GetGroupClinicStats;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class GetGroupClinicStatsHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic1 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Clinic2 = new("33333333-3333-3333-3333-333333333333");
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IPatientStatsReader _patientStats = Substitute.For<IPatientStatsReader>();
    private readonly IAppointmentStatsReader _appointmentStats = Substitute.For<IAppointmentStatsReader>();
    private readonly IRevenueStatsReader _revenueStats = Substitute.For<IRevenueStatsReader>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPerClinicStats()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);

        // Add clinics to the context so names can be resolved
        var clinic1 = Clinic.Create("Al Barsha Vet").Value;
        var clinic2 = Clinic.Create("Jumeirah Vet").Value;

        // Use reflection to set Ids to match our constants
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic1, Clinic1);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic2, Clinic2);
        context.Clinics.AddRange(clinic1, clinic2);

        var group = ClinicGroup.Create("Desert Paws Group", user.Id).Value;
        group.AddClinic(Clinic1);
        group.AddClinic(Clinic2);
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        _patientStats.GetPatientCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int> { [Clinic1] = 15, [Clinic2] = 25 }));

        _appointmentStats.GetAppointmentCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int> { [Clinic1] = 50, [Clinic2] = 80 }));

        _revenueStats.GetRevenueByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, decimal>>.Success(
                new Dictionary<Guid, decimal> { [Clinic1] = 5000m, [Clinic2] = 8000m }));

        var handler = new GetGroupClinicStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupClinicStatsQuery(group.Id, user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var barsha = result.Value.First(c => c.ClinicId == Clinic1);
        barsha.ClinicName.Should().Be("Al Barsha Vet");
        barsha.PatientCount.Should().Be(15);
        barsha.AppointmentCount.Should().Be(50);
        barsha.Revenue.Should().Be(5000m);

        var jumeirah = result.Value.First(c => c.ClinicId == Clinic2);
        jumeirah.ClinicName.Should().Be("Jumeirah Vet");
        jumeirah.PatientCount.Should().Be(25);
        jumeirah.AppointmentCount.Should().Be(80);
        jumeirah.Revenue.Should().Be(8000m);
    }

    [Fact]
    public async Task Handle_NotOwner_ReturnsForbidden()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        var group = ClinicGroup.Create("Some Group", user.Id).Value;
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        var handler = new GetGroupClinicStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupClinicStatsQuery(group.Id, Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_GroupNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new GetGroupClinicStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupClinicStatsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
