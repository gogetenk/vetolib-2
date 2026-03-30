using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
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

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
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
        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
        var command = new LoginCommand("invited@desertpaws.ae", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("MUST_CHANGE_PASSWORD"));
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

        var handler = new LoginHandler(context, _jwtTokenService, Options.Create(new AuthSecurityOptions()));
        var command = new LoginCommand("ADMIN@DesertPaws.AE", "Admin1234!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
