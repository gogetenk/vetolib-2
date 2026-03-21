using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.SwitchClinic;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class SwitchClinicHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherClinicId = new("22222222-2222-2222-2222-222222222222");

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

    [Fact]
    public async Task Handle_UserHasAccessViaGroup_ReturnsNewToken()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);

        var otherClinic = Clinic.Create("Other Clinic").Value;
        // Set the other clinic's Id to our known ID
        typeof(BaseEntity).GetProperty("Id")!.SetValue(otherClinic, OtherClinicId);
        context.Clinics.Add(otherClinic);

        var group = ClinicGroup.Create("My Group", user.Id).Value;
        group.AddClinic(OtherClinicId);
        context.ClinicGroups.Add(group);

        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessTokenForClinic(Arg.Any<User>(), OtherClinicId)
            .Returns("new-jwt-token");
        _jwtTokenService.GenerateRefreshToken().Returns("new-refresh-token");

        var handler = new SwitchClinicHandler(context, _jwtTokenService);
        var command = new SwitchClinicCommand(user.Id, OtherClinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-jwt-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_UserSwitchesToOwnClinic_ReturnsNewToken()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _jwtTokenService.GenerateAccessTokenForClinic(Arg.Any<User>(), FixedClinicId)
            .Returns("jwt-for-own-clinic");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token");

        var handler = new SwitchClinicHandler(context, _jwtTokenService);
        var command = new SwitchClinicCommand(user.Id, FixedClinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-for-own-clinic");
    }

    [Fact]
    public async Task Handle_UserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new SwitchClinicHandler(context, _jwtTokenService);
        var command = new SwitchClinicCommand(user.Id, OtherClinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new SwitchClinicHandler(context, _jwtTokenService);
        var command = new SwitchClinicCommand(Guid.NewGuid(), OtherClinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
