using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.ChangeUserRole;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class ChangeUserRoleHandlerTests
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

    private static User CreateReceptionist(Guid clinicId, string email = "receptionist@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Recep1234!", UserRole.Receptionist);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private static User CreateAdmin(Guid clinicId, string email = "admin@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Admin1234!", UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_PromoteReceptionistToVet_ReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var receptionist = CreateReceptionist(FixedClinicId);
        context.Users.AddRange(admin, receptionist);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context);
        var command = new ChangeUserRoleCommand(admin.Id, receptionist.Id, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == receptionist.Id);
        updated.Role.Should().Be(UserRole.Vet);
    }

    [Fact]
    public async Task Handle_TargetUserNotFound_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context);
        var nonExistentUserId = Guid.NewGuid();
        var command = new ChangeUserRoleCommand(admin.Id, nonExistentUserId, UserRole.Vet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_AdminTriesToChangeOwnRole_ReturnsError()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context);
        var command = new ChangeUserRoleCommand(admin.Id, admin.Id, UserRole.Receptionist);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("own admin role"));
    }

    [Fact]
    public async Task Handle_DemoteVetToAssistant_ReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var admin = CreateAdmin(FixedClinicId);
        var vet = User.Create(FixedClinicId, "vet@desertpaws.ae", "Vet1234!", UserRole.Vet, "VET-001").Value;
        context.Users.AddRange(admin, vet);
        await context.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(context);
        var command = new ChangeUserRoleCommand(admin.Id, vet.Id, UserRole.Assistant);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Users.FirstAsync(u => u.Id == vet.Id);
        updated.Role.Should().Be(UserRole.Assistant);
    }
}
