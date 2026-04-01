using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.ListMyOrganizations;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ListMyOrganizationsHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic2Id = new("22222222-2222-2222-2222-222222222222");
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdmin = Substitute.For<IKeycloakAdminService>();

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
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new ListMyOrganizationsHandler(context, _keycloakAdmin);

        // Act
        var result = await handler.Handle(
            new ListMyOrganizationsQuery(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_UserWithKeycloakId_ReturnsKeycloakOrganizations()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "vet@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        var keycloakUserId = Guid.NewGuid();
        user.SetKeycloakUserId(keycloakUserId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var kcOrgs = new List<KeycloakOrganizationDto>
        {
            new(Guid.NewGuid(), "Al Barsha Vet Clinic", FixedClinicId),
            new(Guid.NewGuid(), "Jumeirah Vet Clinic", Clinic2Id)
        };

        _keycloakAdmin
            .ListUserOrganizationsAsync(keycloakUserId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<KeycloakOrganizationDto>>.Success(kcOrgs));

        var handler = new ListMyOrganizationsHandler(context, _keycloakAdmin);

        // Act
        var result = await handler.Handle(
            new ListMyOrganizationsQuery(user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(o => o.Name == "Al Barsha Vet Clinic" && o.ClinicId == FixedClinicId);
        result.Value.Should().Contain(o => o.Name == "Jumeirah Vet Clinic" && o.ClinicId == Clinic2Id);
    }

    [Fact]
    public async Task Handle_KeycloakFails_FallsBackToClinicGroups()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "vet@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        var keycloakUserId = Guid.NewGuid();
        user.SetKeycloakUserId(keycloakUserId);
        context.Users.Add(user);

        var clinic = Clinic.Create("Desert Paws Clinic").Value;
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic, FixedClinicId);
        context.Clinics.Add(clinic);

        var clinic2 = Clinic.Create("Jumeirah Clinic").Value;
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic2, Clinic2Id);
        context.Clinics.Add(clinic2);

        var group = ClinicGroup.Create("My Group", user.Id).Value;
        group.AddClinic(Clinic2Id);
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        _keycloakAdmin
            .ListUserOrganizationsAsync(keycloakUserId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<KeycloakOrganizationDto>>.Error("Keycloak unavailable"));

        var handler = new ListMyOrganizationsHandler(context, _keycloakAdmin);

        // Act
        var result = await handler.Handle(
            new ListMyOrganizationsQuery(user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(o => o.ClinicId == Clinic2Id);
        result.Value.Should().Contain(o => o.ClinicId == FixedClinicId); // own clinic always included
    }

    [Fact]
    public async Task Handle_NoKeycloakId_FallsBackToClinicGroups()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "vet@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        // No KeycloakUserId set
        context.Users.Add(user);

        var clinic = Clinic.Create("Home Clinic").Value;
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic, FixedClinicId);
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var handler = new ListMyOrganizationsHandler(context, _keycloakAdmin);

        // Act
        var result = await handler.Handle(
            new ListMyOrganizationsQuery(user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ClinicId.Should().Be(FixedClinicId);
        result.Value[0].Name.Should().Be("Home Clinic");

        // Keycloak should NOT have been called
        await _keycloakAdmin.DidNotReceive()
            .ListUserOrganizationsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "vet@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        user.Deactivate();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new ListMyOrganizationsHandler(context, _keycloakAdmin);

        // Act
        var result = await handler.Handle(
            new ListMyOrganizationsQuery(user.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
