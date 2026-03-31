using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.GetGroupRevenueComparison;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class GetGroupRevenueComparisonHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic1 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Clinic2 = new("33333333-3333-3333-3333-333333333333");
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
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
    public async Task Handle_ValidRequest_ReturnsRevenueComparison()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);

        var clinic1 = Clinic.Create("Al Barsha Vet").Value;
        var clinic2 = Clinic.Create("Jumeirah Vet").Value;
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic1, Clinic1);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic2, Clinic2);
        context.Clinics.AddRange(clinic1, clinic2);

        var group = ClinicGroup.Create("Desert Paws Group", user.Id).Value;
        group.AddClinic(Clinic1);
        group.AddClinic(Clinic2);
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        _revenueStats.GetRevenueByClinicIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyDictionary<Guid, decimal>>.Success(
                new Dictionary<Guid, decimal> { [Clinic1] = 5000m, [Clinic2] = 8000m }));

        var handler = new GetGroupRevenueComparisonHandler(context, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupRevenueComparisonQuery(group.Id, user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GroupId.Should().Be(group.Id);
        result.Value.GroupName.Should().Be("Desert Paws Group");
        result.Value.Currency.Should().Be("AED");
        result.Value.Clinics.Should().HaveCount(2);
        result.Value.Clinics.First(c => c.ClinicId == Clinic1).Revenue.Should().Be(5000m);
        result.Value.Clinics.First(c => c.ClinicId == Clinic2).Revenue.Should().Be(8000m);
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

        var handler = new GetGroupRevenueComparisonHandler(context, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupRevenueComparisonQuery(group.Id, Guid.NewGuid()),
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
        var handler = new GetGroupRevenueComparisonHandler(context, _revenueStats);

        // Act
        var result = await handler.Handle(
            new GetGroupRevenueComparisonQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
