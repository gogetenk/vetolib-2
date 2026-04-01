using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Vetolib.Auth.Application.Commands.CreateUser;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class CreateUserHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid KeycloakUserId = new("22222222-2222-2222-2222-222222222222");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdmin = Substitute.For<IKeycloakAdminService>();
    private readonly ILogger<CreateUserHandler> _logger = Substitute.For<ILogger<CreateUserHandler>>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private CreateUserCommand ValidCommand(string email = "newuser@desertpaws.ae") =>
        new(FixedClinicId, email, "StrongP@ss1!", UserRole.Receptionist, null);

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndSyncsWithKeycloak()
    {
        // Arrange
        using var context = BuildContext();
        _keycloakAdmin.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(KeycloakUserId));
        _keycloakAdmin.AddUserToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new CreateUserHandler(context, _logger, _keycloakAdmin);
        var cmd = ValidCommand();

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("newuser@desertpaws.ae");

        var user = await context.Users.FirstAsync();
        user.KeycloakUserId.Should().Be(KeycloakUserId);

        await _keycloakAdmin.Received(1).CreateUserAsync(
            "newuser@desertpaws.ae", "StrongP@ss1!", string.Empty, string.Empty, Arg.Any<CancellationToken>());
        await _keycloakAdmin.Received(1).AddUserToOrganizationAsync(
            KeycloakUserId, FixedClinicId, Arg.Is<IReadOnlyList<string>>(r => r.Contains("Receptionist")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakUnavailable_StillCreatesUserInDb()
    {
        // Arrange
        using var context = BuildContext();
        _keycloakAdmin.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var handler = new CreateUserHandler(context, _logger, _keycloakAdmin);
        var cmd = ValidCommand();

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync();
        user.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_KeycloakCreateUserFails_StillSucceeds_NoKeycloakId()
    {
        // Arrange
        using var context = BuildContext();
        _keycloakAdmin.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("Keycloak is down"));

        var handler = new CreateUserHandler(context, _logger, _keycloakAdmin);
        var cmd = ValidCommand();

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync();
        user.KeycloakUserId.Should().BeNull();
        await _keycloakAdmin.DidNotReceive().AddUserToOrganizationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoKeycloakServiceRegistered_StillCreatesUser()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new CreateUserHandler(context, _logger); // no keycloakAdmin

        var cmd = ValidCommand();

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync();
        user.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var existing = User.Create(FixedClinicId, "dup@desertpaws.ae", "StrongP@ss1!", UserRole.Admin);
        context.Users.Add(existing.Value);
        await context.SaveChangesAsync();

        var handler = new CreateUserHandler(context, _logger, _keycloakAdmin);
        var cmd = ValidCommand("dup@desertpaws.ae");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EMAIL_EXISTS"));
    }

    [Fact]
    public async Task Handle_KeycloakAddToOrgFails_UserStillCreatedWithKeycloakId()
    {
        // Arrange
        using var context = BuildContext();
        _keycloakAdmin.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(KeycloakUserId));
        _keycloakAdmin.AddUserToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Org not found"));

        var handler = new CreateUserHandler(context, _logger, _keycloakAdmin);
        var cmd = ValidCommand();

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync();
        user.KeycloakUserId.Should().Be(KeycloakUserId);
    }
}
