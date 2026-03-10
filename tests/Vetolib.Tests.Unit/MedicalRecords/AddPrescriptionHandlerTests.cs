using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.AddPrescription;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class AddPrescriptionHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid VetId = Guid.NewGuid();
    private static readonly Guid DrugId = Guid.NewGuid();

    private readonly MedicalRecordsDbContext _context;
    private readonly ISender _sender;
    private readonly IPublisher _publisher;
    private readonly AddPrescriptionHandler _handler;

    // Captured after seeding
    private Guid MedicalRecordId { get; set; }

    public AddPrescriptionHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _sender = Substitute.For<ISender>();
        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, _publisher);

        _handler = new AddPrescriptionHandler(_context, _sender, _publisher);

        // Seed a MedicalRecord required by the handler
        SeedMedicalRecord();
    }

    private void SeedMedicalRecord()
    {
        var medicalRecord = MedicalRecord.Create(
            ClinicId, PatientId,
            "Annual wellness checkup",
            "Supportive care",
            "Dr. Al-Rashidi",
            DateTime.UtcNow).Value;

        _context.MedicalRecords.Add(medicalRecord);
        _context.SaveChanges();

        // Capture the auto-generated Id for use in commands
        MedicalRecordId = medicalRecord.Id;
    }

    private AddPrescriptionCommand BuildCommand(
        string userRole = "Vet",
        string? overrideJustification = null,
        Guid? drugCatalogEntryId = null)
        => new(
            ClinicId: ClinicId,
            MedicalRecordId: MedicalRecordId,
            PatientId: PatientId,
            Medication: "Amoxicillin",
            Dosage: "250mg twice daily",
            VetLicenseNumber: "UAE-VET-001",
            VetId: VetId,
            UserRole: userRole,
            DrugCatalogEntryId: drugCatalogEntryId,
            DosageAmount: null,
            OverrideJustification: overrideJustification);

    private void SetupNoInteractions()
        => _sender.Send(
                Arg.Any<CheckInteractionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<InteractionCheckResult>.Success(
                new InteractionCheckResult([], [])));

    private void SetupCriticalInteraction()
        => _sender.Send(
                Arg.Any<CheckInteractionsQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<InteractionCheckResult>.Success(
                new InteractionCheckResult(
                    Alerts:
                    [
                        new InteractionAlert(
                            Severity: InteractionSeverity.Critical,
                            Type: InteractionAlertType.SpeciesContraindication,
                            Message: "This drug is contraindicated for cats — risk of hepatic necrosis",
                            AlternativeDrugIds: [])
                    ],
                    Alternatives: [])));

    // ── RBAC Tests ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenRoleIsAssistant_ReturnsForbidden()
    {
        var cmd = BuildCommand(userRole: "Assistant");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenRoleIsReceptionist_ReturnsForbidden()
    {
        var cmd = BuildCommand(userRole: "Receptionist");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenRoleIsVet_AllowsCreation()
    {
        var cmd = BuildCommand(userRole: "Vet");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRoleIsAdmin_AllowsCreation()
    {
        var cmd = BuildCommand(userRole: "Admin");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Override Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCriticalAlertAndNoJustification_ReturnsError()
    {
        SetupCriticalInteraction();
        var cmd = BuildCommand(drugCatalogEntryId: DrugId, overrideJustification: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("Override justification required"));
    }

    [Fact]
    public async Task Handle_WhenCriticalAlertAndJustificationTooShort_ReturnsError()
    {
        SetupCriticalInteraction();
        var cmd = BuildCommand(drugCatalogEntryId: DrugId, overrideJustification: "Too short");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("Override justification required"));
    }

    [Fact]
    public async Task Handle_WhenCriticalAlertAndValidJustification_SavesWithOverrideFields()
    {
        SetupCriticalInteraction();
        const string justification = "Benefit outweighs risk after owner consultation and blood panel review";
        var cmd = BuildCommand(drugCatalogEntryId: DrugId, overrideJustification: justification);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverrideJustification.Should().Be(justification);
        result.Value.OverrideSeverity.Should().Be(InteractionSeverity.Critical.ToString());
    }

    [Fact]
    public async Task Handle_WhenCriticalAlertAndValidJustification_PublishesPrescriptionOverriddenEvent()
    {
        SetupCriticalInteraction();
        var cmd = BuildCommand(
            drugCatalogEntryId: DrugId,
            overrideJustification: "Emergency use approved after full allergy panel confirmed safe");

        await _handler.Handle(cmd, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<PrescriptionOverriddenEvent>(e =>
                e.PatientId == PatientId &&
                e.ClinicId == ClinicId &&
                e.VetId == VetId &&
                e.OverrideSeverity == InteractionSeverity.Critical),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoInteractionAlerts_SavesPrescriptionWithoutOverride()
    {
        SetupNoInteractions();
        var cmd = BuildCommand(drugCatalogEntryId: DrugId, overrideJustification: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverrideJustification.Should().BeNull();
        result.Value.OverrideSeverity.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNoInteractionAlerts_DoesNotPublishOverriddenEvent()
    {
        SetupNoInteractions();
        var cmd = BuildCommand(drugCatalogEntryId: DrugId);

        await _handler.Handle(cmd, CancellationToken.None);

        await _publisher.DidNotReceive().Publish(
            Arg.Any<PrescriptionOverriddenEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoDrugCatalogEntryId_SkipsInteractionCheck()
    {
        var cmd = BuildCommand(drugCatalogEntryId: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _sender.DidNotReceive().Send(
            Arg.Any<CheckInteractionsQuery>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMedicalRecordNotFound_ReturnsNotFound()
    {
        var cmd = new AddPrescriptionCommand(
            ClinicId: ClinicId,
            MedicalRecordId: Guid.NewGuid(), // does not exist
            PatientId: PatientId,
            Medication: "Amoxicillin",
            Dosage: "250mg",
            VetLicenseNumber: "UAE-VET-001",
            VetId: VetId,
            UserRole: "Vet");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
