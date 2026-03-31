using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Agenda.Application.Commands.CreateFollowUpRule;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateFollowUpRuleValidatorTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private readonly CreateFollowUpRuleValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up");

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyClinicId_FailsValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(Guid.Empty, "Surgery", 10, "Post-surgery follow-up");

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.ClinicId);
    }

    [Fact]
    public void EmptyConsultationType_FailsValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "", 10, "Post-surgery follow-up");

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.ConsultationType);
    }

    [Fact]
    public void ConsultationTypeExceeding100Chars_FailsValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, new string('A', 101), 10, "Post-surgery follow-up");

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.ConsultationType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void InvalidFollowUpDays_FailsValidation(int days)
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", days, "Post-surgery follow-up");

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.FollowUpDays);
    }

    [Fact]
    public void EmptyFollowUpReason_FailsValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "");

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.FollowUpReason);
    }

    [Fact]
    public void FollowUpReasonExceeding200Chars_FailsValidation()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, new string('A', 201));

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.FollowUpReason);
    }
}
