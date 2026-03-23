using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.AI.Application.Commands.AcknowledgeHealthAlert;
using Vetolib.AI.Application.Commands.ConvertAlertToAppointment;
using Vetolib.AI.Application.Commands.DismissHealthAlert;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Application.Queries.GetHealthAlerts;
using Vetolib.AI.Application.Queries.GetPatientHealthAlerts;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class HealthAlertHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PatientId2 = new("33333333-3333-3333-3333-333333333333");

    private readonly AIDbContext _context;

    public HealthAlertHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AIDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AIDbContext(options, clinicContext, publisher);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private HealthAlert CreateAndSaveAlert(
        HealthAlertSeverity severity = HealthAlertSeverity.Medium,
        HealthAlertStatus? statusOverride = null,
        Guid? patientId = null)
    {
        var alert = HealthAlert.Create(
            ClinicId, patientId ?? PatientId,
            HealthAlertType.BreedSpecificScreening, severity,
            "Test Alert", "Test Description", "Recommended Action",
            "TEST_RULE", 50).Value;

        // Apply status overrides for testing edge cases
        if (statusOverride == HealthAlertStatus.Acknowledged)
            alert.Acknowledge();
        else if (statusOverride == HealthAlertStatus.Dismissed)
            alert.Dismiss("pre-dismissed", "Admin");

        _context.HealthAlerts.Add(alert);
        _context.SaveChanges();
        return alert;
    }

    // ── DismissHealthAlertHandler ──────────────────────────────────────────

    [Fact]
    public async Task Dismiss_ExistingNewAlert_ReturnsSuccess()
    {
        var alert = CreateAndSaveAlert();
        var handler = new DismissHealthAlertHandler(_context);

        var result = await handler.Handle(
            new DismissHealthAlertCommand(alert.Id, "Owner declined screening", "Dr. Smith"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _context.HealthAlerts.FindAsync(alert.Id);
        updated!.Status.Should().Be(HealthAlertStatus.Dismissed);
        updated.DismissedReason.Should().Be("Owner declined screening");
    }

    [Fact]
    public async Task Dismiss_NonExistentAlert_ReturnsNotFound()
    {
        var handler = new DismissHealthAlertHandler(_context);

        var result = await handler.Handle(
            new DismissHealthAlertCommand(Guid.NewGuid(), "reason", "name"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Dismiss_AlreadyDismissedAlert_ReturnsError()
    {
        var alert = CreateAndSaveAlert(statusOverride: HealthAlertStatus.Dismissed);
        var handler = new DismissHealthAlertHandler(_context);

        var result = await handler.Handle(
            new DismissHealthAlertCommand(alert.Id, "reason", "name"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── AcknowledgeHealthAlertHandler ──────────────────────────────────────

    [Fact]
    public async Task Acknowledge_NewAlert_ReturnsSuccess()
    {
        var alert = CreateAndSaveAlert();
        var handler = new AcknowledgeHealthAlertHandler(_context);

        var result = await handler.Handle(
            new AcknowledgeHealthAlertCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _context.HealthAlerts.FindAsync(alert.Id);
        updated!.Status.Should().Be(HealthAlertStatus.Acknowledged);
    }

    [Fact]
    public async Task Acknowledge_NonExistentAlert_ReturnsNotFound()
    {
        var handler = new AcknowledgeHealthAlertHandler(_context);

        var result = await handler.Handle(
            new AcknowledgeHealthAlertCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Acknowledge_AlreadyAcknowledgedAlert_ReturnsError()
    {
        var alert = CreateAndSaveAlert(statusOverride: HealthAlertStatus.Acknowledged);
        var handler = new AcknowledgeHealthAlertHandler(_context);

        var result = await handler.Handle(
            new AcknowledgeHealthAlertCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Acknowledge_DismissedAlert_ReturnsError()
    {
        var alert = CreateAndSaveAlert(statusOverride: HealthAlertStatus.Dismissed);
        var handler = new AcknowledgeHealthAlertHandler(_context);

        var result = await handler.Handle(
            new AcknowledgeHealthAlertCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── ConvertAlertToAppointmentHandler ──────────────────────────────────

    [Fact]
    public async Task ConvertToAppointment_NewAlert_ReturnsPreFillDto()
    {
        var alert = CreateAndSaveAlert();
        var handler = new ConvertAlertToAppointmentHandler(_context);

        var result = await handler.Handle(
            new ConvertAlertToAppointmentCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.AlertTitle.Should().Be("Test Alert");
        result.Value.SuggestedNotes.Should().Be("Recommended Action");
    }

    [Fact]
    public async Task ConvertToAppointment_NonExistentAlert_ReturnsNotFound()
    {
        var handler = new ConvertAlertToAppointmentHandler(_context);

        var result = await handler.Handle(
            new ConvertAlertToAppointmentCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ConvertToAppointment_DismissedAlert_ReturnsError()
    {
        var alert = CreateAndSaveAlert(statusOverride: HealthAlertStatus.Dismissed);
        var handler = new ConvertAlertToAppointmentHandler(_context);

        var result = await handler.Handle(
            new ConvertAlertToAppointmentCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ConvertToAppointment_WithNoRecommendedAction_UseDescription()
    {
        var alert = HealthAlert.Create(
            ClinicId, PatientId,
            HealthAlertType.SeniorWellness, HealthAlertSeverity.High,
            "Senior Alert", "Needs checkup", null,
            "SENIOR_RULE", 70).Value;

        _context.HealthAlerts.Add(alert);
        await _context.SaveChangesAsync();

        var handler = new ConvertAlertToAppointmentHandler(_context);

        var result = await handler.Handle(
            new ConvertAlertToAppointmentCommand(alert.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SuggestedNotes.Should().Be("Needs checkup");
    }

    // ── GetHealthAlertsHandler ──────────────────────────────────────────

    [Fact]
    public async Task GetHealthAlerts_ExcludesDismissed()
    {
        CreateAndSaveAlert();
        CreateAndSaveAlert(statusOverride: HealthAlertStatus.Dismissed);
        var handler = new GetHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetHealthAlertsQuery(null, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHealthAlerts_FilterBySeverity()
    {
        CreateAndSaveAlert(severity: HealthAlertSeverity.High);
        CreateAndSaveAlert(severity: HealthAlertSeverity.Low);
        var handler = new GetHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetHealthAlertsQuery(HealthAlertSeverity.High, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Fact]
    public async Task GetHealthAlerts_FilterByPatientId()
    {
        CreateAndSaveAlert(patientId: PatientId);
        CreateAndSaveAlert(patientId: PatientId2);
        var handler = new GetHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetHealthAlertsQuery(null, null, PatientId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be(PatientId);
    }

    [Fact]
    public async Task GetHealthAlerts_OrderedBySeverityDescThenDateDesc()
    {
        CreateAndSaveAlert(severity: HealthAlertSeverity.Low);
        CreateAndSaveAlert(severity: HealthAlertSeverity.High);
        CreateAndSaveAlert(severity: HealthAlertSeverity.Medium);
        var handler = new GetHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetHealthAlertsQuery(null, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].Severity.Should().Be(HealthAlertSeverity.High);
    }

    // ── GetPatientHealthAlertsHandler ──────────────────────────────────────

    [Fact]
    public async Task GetPatientHealthAlerts_IncludesDismissed()
    {
        CreateAndSaveAlert(patientId: PatientId);
        CreateAndSaveAlert(patientId: PatientId, statusOverride: HealthAlertStatus.Dismissed);
        var handler = new GetPatientHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetPatientHealthAlertsQuery(PatientId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPatientHealthAlerts_EmptyPatientId_ReturnsInvalid()
    {
        var handler = new GetPatientHealthAlertsHandler(_context);

        var result = await handler.Handle(
            new GetPatientHealthAlertsQuery(Guid.Empty),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    // ── DismissHealthAlertValidator ──────────────────────────────────────

    [Fact]
    public async Task DismissValidator_EmptyReason_Fails()
    {
        var validator = new DismissHealthAlertValidator();

        var result = await validator.ValidateAsync(
            new DismissHealthAlertCommand(Guid.NewGuid(), "", "Dr. Smith"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }

    [Fact]
    public async Task DismissValidator_ReasonTooLong_Fails()
    {
        var validator = new DismissHealthAlertValidator();

        var result = await validator.ValidateAsync(
            new DismissHealthAlertCommand(Guid.NewGuid(), new string('x', 501), "Dr. Smith"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }

    [Fact]
    public async Task DismissValidator_ValidCommand_Passes()
    {
        var validator = new DismissHealthAlertValidator();

        var result = await validator.ValidateAsync(
            new DismissHealthAlertCommand(Guid.NewGuid(), "Owner declined screening", "Dr. Smith"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DismissValidator_EmptyAlertId_Fails()
    {
        var validator = new DismissHealthAlertValidator();

        var result = await validator.ValidateAsync(
            new DismissHealthAlertCommand(Guid.Empty, "reason", "Dr. Smith"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AlertId");
    }
}
