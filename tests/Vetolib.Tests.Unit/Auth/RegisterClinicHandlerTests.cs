using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Vetolib.Auth.Application;
using Vetolib.Auth.Application.Commands.RegisterClinic;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RegisterClinicHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid KcOrgId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid KcUserId = new("33333333-3333-3333-3333-333333333333");

    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IKeycloakAdminService _keycloakAdminService = Substitute.For<IKeycloakAdminService>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private RegisterClinicHandler BuildHandler(AuthDbContext context, IKeycloakAdminService? keycloak = null)
    {
        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token-value");

        return new RegisterClinicHandler(
            context,
            _jwtTokenService,
            Options.Create(new AuthSecurityOptions()),
            NullLogger<RegisterClinicHandler>.Instance,
            keycloak);
    }

    private static RegisterClinicCommand ValidCommand(string email = "owner@desertpaws.ae") =>
        new("Desert Paws Vet", email, "Secure@1a", "+971501234567", "AE");

    [Fact]
    public async Task Handle_ValidCommand_CreatesClinicAndUser()
    {
        // Arrange
        using var context = BuildContext();
        var handler = BuildHandler(context);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicName.Should().Be("Desert Paws Vet");
        result.Value.AccessToken.Should().Be("jwt-access-token");
        result.Value.RefreshToken.Should().Be("refresh-token-value");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var existingUser = User.Create(FixedClinicId, "owner@desertpaws.ae", "Secure@1a", UserRole.Admin).Value;
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var handler = BuildHandler(context);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EMAIL_EXISTS"));
    }

    [Fact]
    public async Task Handle_WithKeycloak_SyncsOrgAndUser()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdminService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(KcOrgId));
        _keycloakAdminService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(KcUserId));
        _keycloakAdminService.AddUserToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = BuildHandler(context, _keycloakAdminService);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _keycloakAdminService.Received(1).CreateOrganizationAsync("Desert Paws Vet", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _keycloakAdminService.Received(1).CreateUserAsync("owner@desertpaws.ae", "Secure@1a", string.Empty, string.Empty, Arg.Any<CancellationToken>());
        await _keycloakAdminService.Received(1).AddUserToOrganizationAsync(KcUserId, KcOrgId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());

        // Verify Keycloak IDs were persisted
        var clinic = await context.Clinics.IgnoreQueryFilters().FirstAsync();
        clinic.KeycloakOrganizationId.Should().Be(KcOrgId);

        var user = await context.Users.IgnoreQueryFilters().FirstAsync();
        user.KeycloakUserId.Should().Be(KcUserId);
    }

    [Fact]
    public async Task Handle_WithoutKeycloak_SucceedsWithoutSync()
    {
        // Arrange
        using var context = BuildContext();
        var handler = BuildHandler(context, keycloak: null);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdminService.DidNotReceive().CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakOrgCreationFails_RegistrationStillSucceeds()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdminService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("Keycloak unavailable"));

        var handler = BuildHandler(context, _keycloakAdminService);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdminService.DidNotReceive().CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakUserCreationFails_RegistrationStillSucceeds()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdminService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(KcOrgId));
        _keycloakAdminService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("User creation failed"));

        var handler = BuildHandler(context, _keycloakAdminService);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _keycloakAdminService.DidNotReceive().AddUserToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakThrowsException_RegistrationStillSucceeds()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdminService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Result<Guid>>(x => throw new HttpRequestException("Connection refused"));

        var handler = BuildHandler(context, _keycloakAdminService);

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
