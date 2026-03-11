using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Auth.Application.Commands.DismissChecklist;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class DismissChecklistHandlerTests
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private static OnboardingState CreateOnboardingState(Guid userId)
    {
        var result = OnboardingState.Create(userId, "Admin", ClinicId);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public async Task Handle_WhenOnboardingStateExists_DismissesChecklistAndReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var userId = Guid.NewGuid();
        var state = CreateOnboardingState(userId);
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        var handler = new DismissChecklistHandler(context);
        var command = new DismissChecklistCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var saved = await context.OnboardingStates.FirstOrDefaultAsync(o => o.UserId == userId);
        saved.Should().NotBeNull();
        saved!.ChecklistDismissed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOnboardingStateDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = BuildContext();
        var handler = new DismissChecklistHandler(context);
        var command = new DismissChecklistCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenChecklistAlreadyDismissed_StillReturnsSuccess()
    {
        // Arrange
        using var context = BuildContext();
        var userId = Guid.NewGuid();
        var state = CreateOnboardingState(userId);
        state.DismissChecklist(); // already dismissed
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        var handler = new DismissChecklistHandler(context);
        var command = new DismissChecklistCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
