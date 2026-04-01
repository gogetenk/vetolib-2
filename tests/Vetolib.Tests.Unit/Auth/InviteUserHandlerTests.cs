using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Vetolib.Auth.Application.Commands.InviteUser;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class InviteUserHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IKeycloakAdminService _keycloakAdmin = Substitute.For<IKeycloakAdminService>();
    private readonly ILogger<InviteUserHandler> _logger = Substitute.For<ILogger<InviteUserHandler>>();
    private readonly IConfiguration _configuration;

    public InviteUserHandlerTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ClinicName"] = "Desert Paws Veterinary Clinic"
            })
            .Build();
    }

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
    public async Task Handle_ValidInvite_CreatesUserAndSyncsWithKeycloak()
    {
        // Arrange
        using var context = BuildContext();
        var keycloakUserId = Guid.NewGuid();

        _keycloakAdmin.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(keycloakUserId));

        _keycloakAdmin.AddUserToOrganizationAsync(
                keycloakUserId, FixedClinicId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new InviteUserHandler(context, _configuration, _logger, _keycloakAdmin);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("vet@desertpaws.ae");

        var user = await context.Users.FirstAsync(u => u.Email == "vet@desertpaws.ae");
        user.KeycloakUserId.Should().Be(keycloakUserId);

        await _keycloakAdmin.Received(1).CreateUserAsync(
            "vet@desertpaws.ae", Arg.Any<string>(), "Ahmed Al Maktoum", "", Arg.Any<CancellationToken>());
        await _keycloakAdmin.Received(1).AddUserToOrganizationAsync(
            keycloakUserId, FixedClinicId, Arg.Is<IReadOnlyList<string>>(r => r.Contains("Vet")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeycloakCreateFails_StillCreatesUserInDb()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdmin.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("Keycloak unavailable"));

        var handler = new InviteUserHandler(context, _configuration, _logger, _keycloakAdmin);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — DB user created, Keycloak failure is best-effort
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync(u => u.Email == "vet@desertpaws.ae");
        user.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_KeycloakThrowsException_StillCreatesUserInDb()
    {
        // Arrange
        using var context = BuildContext();

        _keycloakAdmin.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Connection refused"));

        var handler = new InviteUserHandler(context, _configuration, _logger, _keycloakAdmin);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — exception caught, user still created
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync(u => u.Email == "vet@desertpaws.ae");
        user.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithoutKeycloakService_CreatesUserNormally()
    {
        // Arrange — no Keycloak service injected (null)
        using var context = BuildContext();

        var handler = new InviteUserHandler(context, _configuration, _logger, keycloakAdminService: null);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync(u => u.Email == "vet@desertpaws.ae");
        user.KeycloakUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var existingUser = User.Invite(FixedClinicId, "vet@desertpaws.ae", "Existing User", "TempPass1!", UserRole.Vet);
        context.Users.Add(existingUser.Value);
        await context.SaveChangesAsync();

        var handler = new InviteUserHandler(context, _configuration, _logger, _keycloakAdmin);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_KeycloakAddToOrgFails_UserStillHasKeycloakId()
    {
        // Arrange
        using var context = BuildContext();
        var keycloakUserId = Guid.NewGuid();

        _keycloakAdmin.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(keycloakUserId));

        _keycloakAdmin.AddUserToOrganizationAsync(
                keycloakUserId, FixedClinicId, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("Org not found"));

        var handler = new InviteUserHandler(context, _configuration, _logger, _keycloakAdmin);
        var command = new InviteUserCommand(FixedClinicId, Guid.NewGuid(), "vet@desertpaws.ae", "Ahmed Al Maktoum", UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — user created with KeycloakUserId even though org assignment failed
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstAsync(u => u.Email == "vet@desertpaws.ae");
        user.KeycloakUserId.Should().Be(keycloakUserId);
    }
}
