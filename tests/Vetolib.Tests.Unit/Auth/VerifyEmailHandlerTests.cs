using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.VerifyEmail;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class VerifyEmailHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

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

    private static User CreateUserWithVerificationToken(Guid clinicId)
    {
        var result = User.Create(clinicId, "user@desertpaws.ae", "Admin1234!", UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_ValidToken_SetsEmailVerified()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateUserWithVerificationToken(FixedClinicId);
        var token = user.EmailVerificationToken!;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new VerifyEmailHandler(context);
        var command = new VerifyEmailCommand(token);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedUser = await context.Users.FirstAsync();
        updatedUser.EmailVerified.Should().BeTrue();
        updatedUser.EmailVerificationToken.Should().BeNull();
        updatedUser.EmailVerificationExpiry.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new VerifyEmailHandler(context);
        var command = new VerifyEmailCommand("non-existent-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TOKEN"));
    }

    [Fact]
    public async Task Handle_AlreadyVerifiedUser_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateUserWithVerificationToken(FixedClinicId);
        var token = user.EmailVerificationToken!;
        // Verify the email first
        user.VerifyEmail(token);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new VerifyEmailHandler(context);
        // The token is now null after verification, so lookup will fail
        var command = new VerifyEmailCommand(token);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
