using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Options;
using Vetolib.Auth.Application;
using Vetolib.Auth.Application.Commands.Login;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class LoginHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdminService = Substitute.For<IKeycloakAdminService>();
    private readonly ILogger<LoginHandler> _logger = Substitute.For<ILogger<LoginHandler>>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private LoginHandler BuildHandler(AuthDbContext context)
    {
        return new LoginHandler(
            context,
            _jwtTokenService,
            Options.Create(new AuthSecurityOptions()),
            _keycloakAdminService,
            _logger);
    }

    private static User CreateActiveUser(Guid clinicId, string email = "admin@desertpaws.ae", string password = "Admin1234!")
    {
        var result = User.Create(clinicId, email, password, UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-access-token");
        result.Value.RefreshToken.Should().Be("refresh-token-value");
        result.Value.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_EmailNotFound_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var handler = BuildHandler(context);
        var command = new LoginCommand("unknown@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_CREDENTIALS"));
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "WrongPass99!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_CREDENTIALS"));
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        user.Deactivate();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("ACCOUNT_DEACTIVATED"));
    }

    [Fact]
    public async Task Handle_LockedUser_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        // Trigger 5 failed attempts to lock the account
        for (var i = 0; i < 5; i++)
            user.RecordFailedLogin();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("ACCOUNT_LOCKED"));
    }

    [Fact]
    public async Task Handle_MustChangePassword_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Invite(FixedClinicId, "invited@desertpaws.ae", "Invited User", "Admin1234!", UserRole.Vet, "Desert Paws Clinic");
        user.IsSuccess.Should().BeTrue();
        context.Users.Add(user.Value);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);
        var command = new LoginCommand("invited@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("MUST_CHANGE_PASSWORD"));
    }

    [Fact]
    public async Task Handle_UnverifiedEmail_ReturnsSuccessWithWarningMessage()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        // User is created with EmailVerified=false by default
        user.EmailVerified.Should().BeFalse();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — login succeeds but with a success message indicating email not verified
        result.IsSuccess.Should().BeTrue();
        result.SuccessMessage.Should().Contain("EMAIL_NOT_VERIFIED");
    }

    [Fact]
    public async Task Handle_VerifiedEmail_ReturnsSuccessWithoutWarning()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        user.VerifyEmail(user.EmailVerificationToken!);
        user.EmailVerified.Should().BeTrue();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — login succeeds without the warning message
        result.IsSuccess.Should().BeTrue();
        result.SuccessMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_EmailLookupIsCaseInsensitive()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId, "admin@desertpaws.ae");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("ADMIN@DesertPaws.AE", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // --- Lazy Keycloak Migration Tests ---

    [Fact]
    public async Task Handle_LegacyUser_MigratedToKeycloakOnLogin()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        user.AuthProvider.Should().Be(AuthProvider.Legacy);
        user.KeycloakUserId.Should().BeNull();
        context.Users.Add(user);

        var clinic = Clinic.Create("Desert Paws");
        clinic.IsSuccess.Should().BeTrue();
        // Set a known Id matching the user's ClinicId
        typeof(BaseEntity).GetProperty("Id")!.SetValue(clinic.Value, FixedClinicId);
        var keycloakOrgId = Guid.NewGuid();
        clinic.Value.SetKeycloakOrganizationId(keycloakOrgId);
        context.Clinics.Add(clinic.Value);
        await context.SaveChangesAsync();

        var keycloakUserId = Guid.NewGuid();
        _keycloakAdminService.CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(keycloakUserId));
        _keycloakAdminService.AddUserToOrganizationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Reload user from context
        var updatedUser = await context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user.Id);
        updatedUser.AuthProvider.Should().Be(AuthProvider.Both);
        updatedUser.KeycloakUserId.Should().Be(keycloakUserId);

        await _keycloakAdminService.Received(1).CreateUserAsync(
            "admin@desertpaws.ae", "Admin1234!", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _keycloakAdminService.Received(1).AddUserToOrganizationAsync(
            keycloakUserId, keycloakOrgId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyMigratedUser_NotReMigrated()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        // Simulate already migrated user
        user.MigrateToKeycloak(Guid.NewGuid());
        user.AuthProvider.Should().Be(AuthProvider.Both);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdminService.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakFailure_DoesNotBlockLogin()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _keycloakAdminService.CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("Keycloak unavailable"));

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — login succeeds despite Keycloak failure
        result.IsSuccess.Should().BeTrue();

        // User should NOT have been migrated
        var updatedUser = await context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user.Id);
        updatedUser.AuthProvider.Should().Be(AuthProvider.Legacy);
        updatedUser.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_KeycloakException_DoesNotBlockLogin()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _keycloakAdminService.CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Connection refused"));

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        var handler = BuildHandler(context);
        var command = new LoginCommand("admin@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — login succeeds even when Keycloak throws exception
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_KeycloakOnlyUser_ReturnsKeycloakAuthRequiredError()
    {
        // Arrange
        using var context = BuildContext();
        var keycloakUserId = Guid.NewGuid();
        var user = User.CreateKeycloakUser(FixedClinicId, "keycloak@desertpaws.ae", UserRole.Admin, keycloakUserId);
        user.IsSuccess.Should().BeTrue();
        user.Value.AuthProvider.Should().Be(AuthProvider.Keycloak);
        context.Users.Add(user.Value);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);
        var command = new LoginCommand("keycloak@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("KEYCLOAK_AUTH_REQUIRED"));
    }
}
