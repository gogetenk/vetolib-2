using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class FollowUpRuleDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidValues_ReturnsSuccess()
    {
        var result = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up");

        result.IsSuccess.Should().BeTrue();
        result.Value.ConsultationType.Should().Be("Surgery");
        result.Value.FollowUpDays.Should().Be(10);
        result.Value.FollowUpReason.Should().Be("Post-surgery follow-up");
        result.Value.IsActive.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = FollowUpRule.Create(Guid.Empty, "Surgery", 10, "Post-surgery follow-up");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyConsultationType_ReturnsInvalid()
    {
        var result = FollowUpRule.Create(ClinicId, "", 10, "Post-surgery follow-up");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "consultationType");
    }

    [Fact]
    public void Create_WithConsultationTypeExceeding100Chars_ReturnsInvalid()
    {
        var longType = new string('A', 101);
        var result = FollowUpRule.Create(ClinicId, longType, 10, "Post-surgery follow-up");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "consultationType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void Create_WithInvalidFollowUpDays_ReturnsInvalid(int days)
    {
        var result = FollowUpRule.Create(ClinicId, "Surgery", days, "Post-surgery follow-up");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "followUpDays");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(180)]
    [InlineData(365)]
    public void Create_WithBoundaryFollowUpDays_ReturnsSuccess(int days)
    {
        var result = FollowUpRule.Create(ClinicId, "Surgery", days, "Post-surgery follow-up");

        result.IsSuccess.Should().BeTrue();
        result.Value.FollowUpDays.Should().Be(days);
    }

    [Fact]
    public void Create_WithEmptyFollowUpReason_ReturnsInvalid()
    {
        var result = FollowUpRule.Create(ClinicId, "Surgery", 10, "");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "followUpReason");
    }

    [Fact]
    public void Create_WithReasonExceeding200Chars_ReturnsInvalid()
    {
        var longReason = new string('A', 201);
        var result = FollowUpRule.Create(ClinicId, "Surgery", 10, longReason);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "followUpReason");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = FollowUpRule.Create(ClinicId, "  Surgery  ", 10, "  Post-surgery follow-up  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.ConsultationType.Should().Be("Surgery");
        result.Value.FollowUpReason.Should().Be("Post-surgery follow-up");
    }

    // ── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidValues_ReturnsSuccess()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;

        var result = rule.Update("Vaccination", 21, "Vaccination booster check");

        result.IsSuccess.Should().BeTrue();
        rule.ConsultationType.Should().Be("Vaccination");
        rule.FollowUpDays.Should().Be(21);
        rule.FollowUpReason.Should().Be("Vaccination booster check");
    }

    [Fact]
    public void Update_WithEmptyConsultationType_ReturnsInvalid()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;

        var result = rule.Update("", 21, "Vaccination booster check");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Update_WithInvalidDays_ReturnsInvalid()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;

        var result = rule.Update("Vaccination", 0, "Vaccination booster check");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    // ── Deactivate ──────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveRule_ReturnsSuccess()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;

        var result = rule.Deactivate();

        result.IsSuccess.Should().BeTrue();
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ReturnsError()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;
        rule.Deactivate();

        var result = rule.Deactivate();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    // ── ToDto ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsCorrectly()
    {
        var rule = FollowUpRule.Create(ClinicId, "Surgery", 10, "Post-surgery follow-up").Value;

        var dto = rule.ToDto();

        dto.Id.Should().Be(rule.Id);
        dto.ConsultationType.Should().Be("Surgery");
        dto.FollowUpDays.Should().Be(10);
        dto.FollowUpReason.Should().Be("Post-surgery follow-up");
        dto.IsActive.Should().BeTrue();
    }
}
