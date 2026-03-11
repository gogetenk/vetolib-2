using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Auth.Application.Commands.ChangePassword;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ChangePasswordHandlerTests
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

    private static User CreateAdminUser(Guid clinicId, string email = "admin@desertpaws.ae", string password = "Admin1234!")
    {
        var result = User.Create(clinicId, email, password, UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_ValidCurrentPassword_ChangesPasswordAndReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new ChangePasswordHandler(context, NullLogger<ChangePasswordHandler>.Instance);
        var cmd = new ChangePasswordCommand(user.Id, "Admin1234!", "NewPass9!");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidCurrentPassword_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new ChangePasswordHandler(context, NullLogger<ChangePasswordHandler>.Instance);
        var cmd = new ChangePasswordCommand(user.Id, "WrongPass1!", "NewPass9!");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_CURRENT_PASSWORD"));
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new ChangePasswordHandler(context, NullLogger<ChangePasswordHandler>.Instance);
        var cmd = new ChangePasswordCommand(Guid.NewGuid(), "Admin1234!", "NewPass9!");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
