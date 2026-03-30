using FluentAssertions;
using Vetolib.Billing.Application.Commands.SubmitEReporting;
using Xunit;

namespace Vetolib.Tests.Unit.Billing;

public class SubmitEReportingValidatorTests
{
    private readonly SubmitEReportingValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new SubmitEReportingCommand(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Period_end_before_period_start_fails()
    {
        var cmd = new SubmitEReportingCommand(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 15));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "PeriodEnd must be >= PeriodStart");
    }

    [Fact]
    public void Same_start_and_end_passes()
    {
        var cmd = new SubmitEReportingCommand(new DateOnly(2026, 1, 15), new DateOnly(2026, 1, 15));
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}
