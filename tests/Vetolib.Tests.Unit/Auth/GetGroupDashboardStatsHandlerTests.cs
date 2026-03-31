using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.GetGroupDashboardStats;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class GetGroupDashboardStatsHandlerTests
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

    private async Task<(Guid userId, Guid groupId)> SeedGroupWithClinics(AuthDbContext context)
    {
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);

        var group = ClinicGroup.Create("Desert Paws Group", user.Id).Value;
        group.AddClinic(Clinic1);
        group.AddClinic(Clinic2);
        context.ClinicGroups.Add(group);

        await context.SaveChangesAsync();
        return (user.Id, group.Id);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAggregatedStats()
    {
        // Arrange
        using var context = BuildContext();
        var (userId, groupId) = await SeedGroupWithClinics(context);

        _patientStats.GetPatientCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int> { [Clinic1] = 15, [Clinic2] = 25 }));

        _appointmentStats.GetAppointmentCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(
                new Dictionary<Guid, int> { [Clinic1] = 50, [Clinic2] = 80 }));

        _revenueStats.GetRevenueByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, decimal>>.Success(
                new Dictionary<Guid, decimal> { [Clinic1] = 5000m, [Clinic2] = 8000m }));

        var handler = new GetGroupDashboardStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(new GetGroupDashboardStatsQuery(groupId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GroupId.Should().Be(groupId);
        result.Value.GroupName.Should().Be("Desert Paws Group");
        result.Value.TotalClinics.Should().Be(2);
        result.Value.TotalPatients.Should().Be(40);
        result.Value.TotalAppointments.Should().Be(130);
        result.Value.TotalRevenue.Should().Be(13000m);
        result.Value.Currency.Should().Be("AED");
    }

    [Fact]
    public async Task Handle_GroupNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new GetGroupDashboardStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupDashboardStatsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_NotOwner_ReturnsForbidden()
    {
        // Arrange
        using var context = BuildContext();
        var (_, groupId) = await SeedGroupWithClinics(context);
        var handler = new GetGroupDashboardStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act — different user
        var result = await handler.Handle(
            new GetGroupDashboardStatsQuery(groupId, Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_EmptyGroup_ReturnsZeroStats()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        var group = ClinicGroup.Create("Empty Group", user.Id).Value;
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        _patientStats.GetPatientCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(new Dictionary<Guid, int>()));
        _appointmentStats.GetAppointmentCountsByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, int>>.Success(new Dictionary<Guid, int>()));
        _revenueStats.GetRevenueByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, decimal>>.Success(new Dictionary<Guid, decimal>()));

        var handler = new GetGroupDashboardStatsHandler(context, _patientStats, _appointmentStats, _revenueStats);

        // Act
        var result = await handler.Handle(new GetGroupDashboardStatsQuery(group.Id, user.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalClinics.Should().Be(0);
        result.Value.TotalPatients.Should().Be(0);
        result.Value.TotalAppointments.Should().Be(0);
        result.Value.TotalRevenue.Should().Be(0m);
    }
}
