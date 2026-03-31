using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.AddClinicToGroup;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class AddClinicToGroupHandlerTests
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

    [Fact]
    public async Task Handle_NonOwnerAddsClinic_ReturnsForbidden()
    {
        // Arrange
        using var context = BuildContext();
        var owner = User.Create(FixedClinicId, "owner@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        var attacker = User.Create(FixedClinicId, "attacker@evil.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(owner);
        context.Users.Add(attacker);

        var group = ClinicGroup.Create("Owner Group", owner.Id).Value;
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        var handler = new AddClinicToGroupHandler(context);
        // Attacker tries to add a clinic to someone else's group — IDOR attempt
        var command = new AddClinicToGroupCommand(group.Id, Guid.NewGuid(), attacker.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_OwnerRequest_PassesOwnershipCheck()
    {
        // Arrange — verify that owner passes the ownership check (gets past Forbidden)
        // The handler may fail later at clinic validation, but it must NOT return Forbidden
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);

        var group = ClinicGroup.Create("My Group", user.Id).Value;
        context.ClinicGroups.Add(group);
        await context.SaveChangesAsync();

        var handler = new AddClinicToGroupHandler(context);
        // ClinicId that doesn't exist — will return NotFound("Clinic not found"), not Forbidden
        var command = new AddClinicToGroupCommand(group.Id, Guid.NewGuid(), user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — should be NotFound (clinic doesn't exist), NOT Forbidden
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain("Clinic not found");
    }

    [Fact]
    public async Task Handle_GroupNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new AddClinicToGroupHandler(context);
        var command = new AddClinicToGroupCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
