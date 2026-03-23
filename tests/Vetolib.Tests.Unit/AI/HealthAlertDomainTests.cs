using Ardalis.Result;
using FluentAssertions;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class HealthAlertDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");

    private static Result<HealthAlert> CreateValidAlert(
        HealthAlertType alertType = HealthAlertType.BreedSpecificScreening,
        HealthAlertSeverity severity = HealthAlertSeverity.Medium,
        int riskScore = 50)
    {
        return HealthAlert.Create(
            ClinicId, PatientId, alertType, severity,
            "Test Alert", "Test Description", "Test Action",
            "TEST_RULE", riskScore);
    }

    // ── Create ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = CreateValidAlert();

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.Status.Should().Be(HealthAlertStatus.New);
        result.Value.Title.Should().Be("Test Alert");
        result.Value.RuleId.Should().Be("TEST_RULE");
        result.Value.RiskScore.Should().Be(50);
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = HealthAlert.Create(
            Guid.Empty, PatientId, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "Title", "Desc", null, "RULE", 30);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "ClinicId");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = HealthAlert.Create(
            ClinicId, Guid.Empty, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "Title", "Desc", null, "RULE", 30);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "PatientId");
    }

    [Fact]
    public void Create_WithEmptyTitle_ReturnsInvalid()
    {
        var result = HealthAlert.Create(
            ClinicId, PatientId, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "", "Desc", null, "RULE", 30);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "Title");
    }

    [Fact]
    public void Create_WithEmptyDescription_ReturnsInvalid()
    {
        var result = HealthAlert.Create(
            ClinicId, PatientId, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "Title", "", null, "RULE", 30);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "Description");
    }

    [Fact]
    public void Create_WithEmptyRuleId_ReturnsInvalid()
    {
        var result = HealthAlert.Create(
            ClinicId, PatientId, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "Title", "Desc", null, "", 30);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "RuleId");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithInvalidRiskScore_ReturnsInvalid(int riskScore)
    {
        var result = CreateValidAlert(riskScore: riskScore);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "RiskScore");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(50)]
    public void Create_WithValidRiskScore_ReturnsSuccess(int riskScore)
    {
        var result = CreateValidAlert(riskScore: riskScore);

        result.IsSuccess.Should().BeTrue();
        result.Value.RiskScore.Should().Be(riskScore);
    }

    [Fact]
    public void Create_WithMultipleErrors_ReturnsAllErrors()
    {
        var result = HealthAlert.Create(
            Guid.Empty, Guid.Empty, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Low, "", "", null, "", -1);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCountGreaterOrEqualTo(5);
    }

    // ── Dismiss ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Dismiss_NewAlert_ReturnsSuccess()
    {
        var alert = CreateValidAlert().Value;

        var result = alert.Dismiss("No longer relevant", "Dr. Smith");

        result.IsSuccess.Should().BeTrue();
        alert.Status.Should().Be(HealthAlertStatus.Dismissed);
        alert.DismissedReason.Should().Be("No longer relevant");
        alert.DismissedByName.Should().Be("Dr. Smith");
        alert.DismissedAt.Should().NotBeNull();
    }

    [Fact]
    public void Dismiss_AlreadyDismissed_ReturnsError()
    {
        var alert = CreateValidAlert().Value;
        alert.Dismiss("reason", "name");

        var result = alert.Dismiss("again", "name2");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Dismiss_WithEmptyReason_ReturnsInvalid()
    {
        var alert = CreateValidAlert().Value;

        var result = alert.Dismiss("", "Dr. Smith");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Dismiss_WithEmptyName_ReturnsInvalid()
    {
        var alert = CreateValidAlert().Value;

        var result = alert.Dismiss("reason", "");

        result.IsSuccess.Should().BeFalse();
    }

    // ── Acknowledge ──────────────────────────────────────────────────────────────

    [Fact]
    public void Acknowledge_NewAlert_ReturnsSuccess()
    {
        var alert = CreateValidAlert().Value;

        var result = alert.Acknowledge();

        result.IsSuccess.Should().BeTrue();
        alert.Status.Should().Be(HealthAlertStatus.Acknowledged);
        alert.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public void Acknowledge_AlreadyAcknowledged_ReturnsError()
    {
        var alert = CreateValidAlert().Value;
        alert.Acknowledge();

        var result = alert.Acknowledge();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Acknowledge_DismissedAlert_ReturnsError()
    {
        var alert = CreateValidAlert().Value;
        alert.Dismiss("reason", "name");

        var result = alert.Acknowledge();

        result.IsSuccess.Should().BeFalse();
    }

    // ── MarkScheduled ──────────────────────────────────────────────────────────

    [Fact]
    public void MarkScheduled_NewAlert_ReturnsSuccess()
    {
        var alert = CreateValidAlert().Value;
        var appointmentId = Guid.NewGuid();

        var result = alert.MarkScheduled(appointmentId);

        result.IsSuccess.Should().BeTrue();
        alert.Status.Should().Be(HealthAlertStatus.Scheduled);
        alert.ConvertedToAppointmentId.Should().Be(appointmentId);
    }

    [Fact]
    public void MarkScheduled_AcknowledgedAlert_ReturnsSuccess()
    {
        var alert = CreateValidAlert().Value;
        alert.Acknowledge();
        var appointmentId = Guid.NewGuid();

        var result = alert.MarkScheduled(appointmentId);

        result.IsSuccess.Should().BeTrue();
        alert.Status.Should().Be(HealthAlertStatus.Scheduled);
    }

    [Fact]
    public void MarkScheduled_DismissedAlert_ReturnsError()
    {
        var alert = CreateValidAlert().Value;
        alert.Dismiss("reason", "name");

        var result = alert.MarkScheduled(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void MarkScheduled_WithEmptyAppointmentId_ReturnsInvalid()
    {
        var alert = CreateValidAlert().Value;

        var result = alert.MarkScheduled(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
    }
}
