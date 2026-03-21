using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.CreateClinicGroup;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class CreateClinicGroupHandlerTests
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
    public async Task Handle_ValidRequest_CreatesGroupSuccessfully()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new CreateClinicGroupHandler(context);
        var command = new CreateClinicGroupCommand("Desert Paws Group", user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Desert Paws Group");
        result.Value.OwnerUserId.Should().Be(user.Id);
        result.Value.Clinics.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsInvalid()
    {
        // Arrange
        using var context = BuildContext();
        var user = User.Create(FixedClinicId, "admin@desertpaws.ae", "Admin1234!", UserRole.Admin).Value;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new CreateClinicGroupHandler(context);
        var command = new CreateClinicGroupCommand("", user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new CreateClinicGroupHandler(context);
        var command = new CreateClinicGroupCommand("Some Group", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
