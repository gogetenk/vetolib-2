using FluentAssertions;
using Vetolib.Auth.Application.Commands.CompleteOnboardingStep;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class CompleteOnboardingStepValidatorTests
{
    private readonly CompleteOnboardingStepValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CompleteOnboardingStepCommand(Guid.NewGuid(), "step-1");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        var cmd = new CompleteOnboardingStepCommand(Guid.Empty, "step-1");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Empty_step_id_fails()
    {
        var cmd = new CompleteOnboardingStepCommand(Guid.NewGuid(), "");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "StepId is required");
    }
}
