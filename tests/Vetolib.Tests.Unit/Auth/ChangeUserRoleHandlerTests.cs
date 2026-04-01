using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Auth.Application.Commands.ChangeUserRole;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ChangeUserRoleHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid FixedKeycloakOrgId = new("22222222-2222-2222-2222-222222222222");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdmin = Substitute.For<IKeycloakAdminService>();
    private readonly ILogger<ChangeUserRoleHandler> _logger = Substitute.For<ILogger<ChangeUserRoleHandler>>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private static User CreateReceptionist(Guid clinicId, string email = "receptionist@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Recep1234!", UserRole.Receptionist);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private static User CreateAdmin(Guid clinicId, string email = "admin@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Admin1234!", UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_PromoteReceptionistToVet_ReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.Role.Should().Be(UserRole.Vet);
    }

    [Fact]
    public async Task Handle_TargetUserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var nonExistentUserId = Guid.NewGuid();
        var command = new ChangeUserRoleCommand(admin.Id, nonExistentUserId, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_AdminTriesToChangeOwnRole_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, admin.Id, UserRole.Receptionist);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("own admin role"));
    }

    [Fact]
    public async Task Handle_DemoteVetToAssistant_ReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var vet = User.Create(FixedClinicId, "vet@desertpaws.ae", "Vet1234!", UserRole.Vet, "VET-001").Value;
        context.Users.AddRange(admin, vet);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, vet.Id, UserRole.Assistant);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == vet.Id);
        updated.Role.Should().Be(UserRole.Assistant);
    }

    [Fact]
    public async Task Handle_UserWithKeycloakId_CallsKeycloakUpdateRoles()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId, "kc-role@desertpaws.ae");
        var keycloakId = Guid.NewGuid();
        receptionist.SetKeycloakUserId(keycloakId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        _keycloakAdmin.ListUserOrganizationsAsync(keycloakId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<KeycloakOrganizationDto>>.Success(
                new List<KeycloakOrganizationDto>
                {
                    new(FixedKeycloakOrgId, "Desert Paws", FixedClinicId)
                }));

        _keycloakAdmin.UpdateUserRolesAsync(keycloakId, FixedKeycloakOrgId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdmin.Received(1).UpdateUserRolesAsync(
            keycloakId,
            FixedKeycloakOrgId,
            Arg.Is<IReadOnlyList<string>>(r => r.Count == 1 && r[0] == "Vet"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserWithoutKeycloakId_DoesNotCallKeycloak()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdmin.DidNotReceive().ListUserOrganizationsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _keycloakAdmin.DidNotReceive().UpdateUserRolesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakUpdateRolesFails_StillReturnsSuccess()
    {
        // Arrange — Keycloak failure is best-effort, DB is source of truth
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId, "kc-fail@desertpaws.ae");
        var keycloakId = Guid.NewGuid();
        receptionist.SetKeycloakUserId(keycloakId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        _keycloakAdmin.ListUserOrganizationsAsync(keycloakId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<KeycloakOrganizationDto>>.Success(
                new List<KeycloakOrganizationDto>
                {
                    new(FixedKeycloakOrgId, "Desert Paws", FixedClinicId)
                }));

        _keycloakAdmin.UpdateUserRolesAsync(keycloakId, FixedKeycloakOrgId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Keycloak unavailable"));

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — still success because DB role change worked
        result.IsSuccess.Should().BeTrue();
        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.Role.Should().Be(UserRole.Vet);
    }

    [Fact]
    public async Task Handle_KeycloakOrgNotFound_StillReturnsSuccess()
    {
        // Arrange — no matching org for the user's clinic
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId, "kc-noorg@desertpaws.ae");
        var keycloakId = Guid.NewGuid();
        receptionist.SetKeycloakUserId(keycloakId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        _keycloakAdmin.ListUserOrganizationsAsync(keycloakId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<KeycloakOrganizationDto>>.Success(
                new List<KeycloakOrganizationDto>())); // empty — no matching org

        var handler = new ChangeUserRoleHandler(context, _keycloakAdmin, _logger);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — still success, role updated in DB
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdmin.DidNotReceive().UpdateUserRolesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }
}
