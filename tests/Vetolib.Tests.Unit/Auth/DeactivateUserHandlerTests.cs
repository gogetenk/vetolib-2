using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vetolib.Auth.Application.Commands.DeactivateUser;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DeactivateUserHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdmin = Substitute.For<IKeycloakAdminService>();
    private readonly ILogger<DeactivateUserHandler> _logger = Substitute.For<ILogger<DeactivateUserHandler>>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private static User CreateAdmin(Guid clinicId, string email = "admin@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Admin1234!", UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private static User CreateReceptionist(Guid clinicId, string email = "receptionist@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Recep1234!", UserRole.Receptionist);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_DeactivateActiveUser_SetsIsActiveFalse()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, receptionist.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TargetUserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var nonExistentUserId = Guid.NewGuid();
        var command = new DeactivateUserCommand(admin.Id, nonExistentUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_AdminTriesToDeactivateThemselves_ReturnsInvalid()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, admin.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "targetUserId");
    }

    [Fact]
    public async Task Handle_DeactivateAlreadyInactiveUser_StillSucceeds()
    {
        // Arrange — the handler calls Deactivate() regardless; IsActive = false is idempotent at domain level
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId);
        receptionist.Deactivate(); // already deactivated
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, receptionist.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — handler does not check pre-existing state, just calls Deactivate()
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UserWithKeycloakId_CallsKeycloakDeactivate()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId, "kc-user@desertpaws.ae");
        var keycloakId = Guid.NewGuid();
        receptionist.SetKeycloakUserId(keycloakId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        _keycloakAdmin.DeactivateUserAsync(keycloakId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, receptionist.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdmin.Received(1).DeactivateUserAsync(keycloakId, Arg.Any<CancellationToken>());
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

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, receptionist.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdmin.DidNotReceive().DeactivateUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakDeactivateFails_StillReturnsSuccess()
    {
        // Arrange — Keycloak failure is best-effort, DB is source of truth
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId, "kc-fail@desertpaws.ae");
        var keycloakId = Guid.NewGuid();
        receptionist.SetKeycloakUserId(keycloakId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        _keycloakAdmin.DeactivateUserAsync(keycloakId, Arg.Any<CancellationToken>())
            .Returns(Result.Error("Keycloak unavailable"));

        var handler = new DeactivateUserHandler(context, _keycloakAdmin, _logger);
        var command = new DeactivateUserCommand(admin.Id, receptionist.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — still success because DB deactivation worked
        result.IsSuccess.Should().BeTrue();
        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.IsActive.Should().BeFalse();
    }
}
