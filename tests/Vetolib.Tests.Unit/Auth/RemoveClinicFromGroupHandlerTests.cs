using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class RemoveClinicFromGroupHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherClinicId = new("22222222-2222-2222-2222-222222222222");
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext(string dbName)
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    [Fact]
    public async Task Handle_OwnerRemovesClinic_Succeeds()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        Guid groupId, userId;

        using (var setupCtx = BuildContext(dbName))
        {
            var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
            setupCtx.Users.Add(user);

            var group = ClinicGroup.Create("My Group", user.Id).Value;
            group.AddClinic(OtherClinicId);
            setupCtx.ClinicGroups.Add(group);
            await setupCtx.SaveChangesAsync();

            groupId = group.Id;
            userId = user.Id;
        }

        using var context = BuildContext(dbName);
        var handler = new RemoveClinicFromGroupHandler(context);
        var command = new RemoveClinicFromGroupCommand(groupId, OtherClinicId, userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonOwnerRemovesClinic_ReturnsForbidden()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        Guid groupId, attackerId;

        using (var setupCtx = BuildContext(dbName))
        {
            var owner = User.Create(FixedClinicId, "owner@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
            var attacker = User.Create(FixedClinicId, "attacker@evil.ae", "Admin1234!", UserRole.Admin).Value;
            setupCtx.Users.Add(owner);
            setupCtx.Users.Add(attacker);

            var group = ClinicGroup.Create("Owner Group", owner.Id).Value;
            group.AddClinic(OtherClinicId);
            setupCtx.ClinicGroups.Add(group);
            await setupCtx.SaveChangesAsync();

            groupId = group.Id;
            attackerId = attacker.Id;
        }

        using var context = BuildContext(dbName);
        var handler = new RemoveClinicFromGroupHandler(context);
        var command = new RemoveClinicFromGroupCommand(groupId, OtherClinicId, attackerId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_GroupNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext(Guid.NewGuid().ToString());
        var handler = new RemoveClinicFromGroupHandler(context);
        var command = new RemoveClinicFromGroupCommand(Guid.NewGuid(), OtherClinicId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
