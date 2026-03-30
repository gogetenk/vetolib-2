using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.RefreshToken;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;
using DomainRefreshToken = Vetolib.Auth.Application.Domain.RefreshToken;

namespace Vetolib.Tests.Unit.Auth;

public class RefreshTokenHandlerTests
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

    private static DomainRefreshToken CreateValidRefreshToken(Guid userId, string token = "valid-refresh-token")
    {
        var result = DomainRefreshToken.Create(userId, token);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokenPair()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        var refreshToken = CreateValidRefreshToken(user.Id, "old-refresh-token");
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("new-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("new-refresh-token");

        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("old-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("nonexistent-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_REFRESH_TOKEN"));
    }

    [Fact]
    public async Task Handle_RevokedToken_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        context.Users.Add(user);
        var refreshToken = CreateValidRefreshToken(user.Id, "revoked-token");
        refreshToken.Revoke();
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("revoked-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_REFRESH_TOKEN"));
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsAccountDeactivatedError()
    {
        // Arrange — Fix 1: deactivated users must NOT be able to refresh tokens
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        user.Deactivate();
        context.Users.Add(user);
        var refreshToken = CreateValidRefreshToken(user.Id, "token-for-deactivated-user");
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("token-for-deactivated-user");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ACCOUNT_DEACTIVATED"));

        // The old token should still be revoked (rotation happened before user check)
        var storedToken = await context.RefreshTokens.FirstAsync(rt => rt.Token == "token-for-deactivated-user");
        storedToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeactivatedUser_NoNewTokenCreated()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateActiveUser(FixedClinicId);
        user.Deactivate();
        context.Users.Add(user);
        var refreshToken = CreateValidRefreshToken(user.Id, "deactivated-token");
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("deactivated-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — no new refresh token should have been created
        var tokenCount = await context.RefreshTokens.CountAsync();
        tokenCount.Should().Be(1); // only the original (now revoked) token
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange — token exists but user was deleted
        using var context = BuildContext();
        var orphanToken = CreateValidRefreshToken(Guid.NewGuid(), "orphan-token");
        context.RefreshTokens.Add(orphanToken);
        await context.SaveChangesAsync();

        var handler = new RefreshTokenHandler(context, _jwtTokenService);
        var command = new RefreshTokenCommand("orphan-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_REFRESH_TOKEN"));
    }
}
