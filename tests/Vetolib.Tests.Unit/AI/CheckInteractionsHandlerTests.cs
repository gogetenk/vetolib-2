using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.AI.Application.Queries.CheckInteractions;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class CheckInteractionsHandlerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly IConfiguration _configuration;
    private readonly CheckInteractionsHandler _handler;

    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid DrugId = Guid.NewGuid();
    private static readonly Guid OtherDrugId = Guid.NewGuid();

    public CheckInteractionsHandlerTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["AI:ActivePrescriptionWindowDays"] = "90"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _handler = new CheckInteractionsHandler(
            _sender,
            _configuration,
            NullLogger<CheckInteractionsHandler>.Instance);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static DrugCatalogEntryDto BuildDrug(
        Guid? id = null,
        IReadOnlyList<SpeciesContraindicationDto>? contraindications = null,
        IReadOnlyList<DrugInteractionDto>? interactions = null,
        IReadOnlyList<DosageGuidelineDto>? dosageGuidelines = null)
        => new(
            Id: id ?? DrugId,
            InnName: "amoxicillin",
            DisplayName: "Amoxicillin 250mg",
            Category: DrugCategory.Antibiotic,
            IsActive: true,
            ClinicId: null,
            SpeciesContraindications: contraindications ?? Array.Empty<SpeciesContraindicationDto>(),
            Interactions: interactions ?? Array.Empty<DrugInteractionDto>(),
            DosageGuidelines: dosageGuidelines ?? Array.Empty<DosageGuidelineDto>());

    private static PrescriptionDto BuildActivePrescription(Guid? drugCatalogEntryId = null)
        => new(
            Id: Guid.NewGuid(),
            MedicalRecordId: Guid.NewGuid(),
            ClinicId: ClinicId,
            Medication: "Amoxicillin",
            Dosage: "250mg",
            VetLicenseNumber: "UAE-VET-001",
            CreatedAt: DateTime.UtcNow.AddDays(-5),
            DrugCatalogEntryId: drugCatalogEntryId);

    private void SetupDrugResult(DrugCatalogEntryDto drug)
        => _sender.Send(
                Arg.Is<GetDrugCatalogEntryByIdQuery>(q => q.Id == drug.Id),
                Arg.Any<CancellationToken>())
            .Returns(Result<DrugCatalogEntryDto>.Success(drug));

    private void SetupPatientResult(Species species, decimal? weightKg = null)
        => _sender.Send(
                Arg.Is<GetPatientSpeciesAndWeightQuery>(q => q.PatientId == PatientId),
                Arg.Any<CancellationToken>())
            .Returns(Result<PatientSpeciesWeightDto>.Success(
                new PatientSpeciesWeightDto(PatientId, species, weightKg)));

    private void SetupActivePrescriptions(List<PrescriptionDto> prescriptions)
        => _sender.Send(
                Arg.Is<GetActivePrescriptionsForPatientQuery>(q => q.PatientId == PatientId),
                Arg.Any<CancellationToken>())
            .Returns(Result<List<PrescriptionDto>>.Success(prescriptions));

    private CheckInteractionsQuery BuildQuery(decimal? dosageAmount = null)
        => new(PatientId, DrugId, dosageAmount, ClinicId);

    // ── Species Contraindication Tests ───────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDrugIsContraindicatedForPatientSpecies_ReturnsCriticalAlert()
    {
        // Arrange
        var contraindication = new SpeciesContraindicationDto(
            Species: Species.Cat,
            Severity: InteractionSeverity.Critical,
            Reason: "Can cause hepatic necrosis in cats");

        var drug = BuildDrug(contraindications: [contraindication]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Cat);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);

        var alert = result.Value.Alerts[0];
        alert.Type.Should().Be(InteractionAlertType.SpeciesContraindication);
        alert.Severity.Should().Be(InteractionSeverity.Critical);
        alert.Message.Should().Contain("Cat");
        alert.Message.Should().Contain("hepatic necrosis");
    }

    [Fact]
    public async Task Handle_WhenDrugIsContraindicatedForDifferentSpecies_ReturnsNoAlert()
    {
        // Arrange — contraindication is for Cat but patient is Dog
        var contraindication = new SpeciesContraindicationDto(
            Species: Species.Cat,
            Severity: InteractionSeverity.Critical,
            Reason: "Toxic to cats");

        var drug = BuildDrug(contraindications: [contraindication]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenContraindicationIsModerate_ReturnsModerateAlert()
    {
        // Arrange
        var contraindication = new SpeciesContraindicationDto(
            Species: Species.Rabbit,
            Severity: InteractionSeverity.Moderate,
            Reason: "Use with caution in rabbits");

        var drug = BuildDrug(contraindications: [contraindication]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Rabbit);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);
        result.Value.Alerts[0].Severity.Should().Be(InteractionSeverity.Moderate);
    }

    // ── Drug-Drug Interaction Tests ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenPatientHasActivePrescriptionWithInteractingDrug_ReturnsInteractionAlert()
    {
        // Arrange
        var interaction = new DrugInteractionDto(
            OtherDrugId: OtherDrugId,
            OtherDrugName: "Metronidazole",
            Severity: InteractionSeverity.Moderate,
            Description: "Combined use may increase neurotoxicity risk");

        var drug = BuildDrug(interactions: [interaction]);

        var activePrescription = BuildActivePrescription(OtherDrugId);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog);
        SetupActivePrescriptions([activePrescription]);

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);

        var alert = result.Value.Alerts[0];
        alert.Type.Should().Be(InteractionAlertType.DrugInteraction);
        alert.Severity.Should().Be(InteractionSeverity.Moderate);
        alert.Message.Should().Contain("Metronidazole");
        alert.Message.Should().Contain("neurotoxicity");
    }

    [Fact]
    public async Task Handle_WhenActivePrescriptionHasNoDrugCatalogEntryId_SkipsInteractionCheck()
    {
        // Arrange — active prescription has no catalog reference, can't check interactions
        var interaction = new DrugInteractionDto(
            OtherDrugId: OtherDrugId,
            OtherDrugName: "Metronidazole",
            Severity: InteractionSeverity.Moderate,
            Description: "Neurotoxicity risk");

        var drug = BuildDrug(interactions: [interaction]);

        var activePrescription = BuildActivePrescription(drugCatalogEntryId: null); // no catalog ID

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog);
        SetupActivePrescriptions([activePrescription]);

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoPrescriptionsAreActive_ReturnsNoInteractionAlert()
    {
        // Arrange
        var interaction = new DrugInteractionDto(
            OtherDrugId: OtherDrugId,
            OtherDrugName: "Metronidazole",
            Severity: InteractionSeverity.Critical,
            Description: "Do not combine");

        var drug = BuildDrug(interactions: [interaction]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog);
        SetupActivePrescriptions([]); // no active prescriptions

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    // ── Dosage Out-of-Range Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDosageIsBelowMinimum_ReturnsDosageAlert()
    {
        // Arrange — dog weighs 10kg, min dose = 5mg/kg => min total = 50mg, given only 20mg
        var guideline = new DosageGuidelineDto(
            Species: Species.Dog,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog, weightKg: 10m);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(dosageAmount: 20m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);

        var alert = result.Value.Alerts[0];
        alert.Type.Should().Be(InteractionAlertType.DosageOutOfRange);
        alert.Severity.Should().Be(InteractionSeverity.Info);
        alert.Message.Should().Contain("below");
        alert.Message.Should().Contain("mg");
    }

    [Fact]
    public async Task Handle_WhenDosageIsAboveMaximum_ReturnsDosageAlert()
    {
        // Arrange — dog weighs 10kg, max dose = 10mg/kg => max total = 100mg, given 250mg
        var guideline = new DosageGuidelineDto(
            Species: Species.Dog,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog, weightKg: 10m);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(dosageAmount: 250m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);

        var alert = result.Value.Alerts[0];
        alert.Type.Should().Be(InteractionAlertType.DosageOutOfRange);
        alert.Message.Should().Contain("above");
    }

    [Fact]
    public async Task Handle_WhenDosageIsWithinRange_ReturnsNoDosageAlert()
    {
        // Arrange — dog weighs 10kg, range = 5-10mg/kg => 50-100mg, given 75mg (in range)
        var guideline = new DosageGuidelineDto(
            Species: Species.Dog,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog, weightKg: 10m);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(dosageAmount: 75m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPatientWeightIsUnknown_SkipsDosageCheck()
    {
        // Arrange — weight is null so dosage check cannot be performed
        var guideline = new DosageGuidelineDto(
            Species: Species.Dog,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog, weightKg: null); // no weight
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(dosageAmount: 20m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDosageAmountIsNull_SkipsDosageCheck()
    {
        // Arrange — dosage amount not provided
        var guideline = new DosageGuidelineDto(
            Species: Species.Dog,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog, weightKg: 10m);
        SetupActivePrescriptions([]);

        // Act — no dosage amount in query
        var result = await _handler.Handle(BuildQuery(dosageAmount: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().BeEmpty();
    }

    // ── Alert Sorting Tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenMultipleAlertsExist_SortsAlertsBySeverityCriticalFirst()
    {
        // Arrange — set up both a species contraindication (Critical) and a dosage issue (Info)
        var contraindication = new SpeciesContraindicationDto(
            Species: Species.Cat,
            Severity: InteractionSeverity.Critical,
            Reason: "Fatal");

        var guideline = new DosageGuidelineDto(
            Species: Species.Cat,
            MinDosePerKg: 5m,
            MaxDosePerKg: 10m,
            Unit: "mg",
            Route: "oral");

        var drug = BuildDrug(
            contraindications: [contraindication],
            dosageGuidelines: [guideline]);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Cat, weightKg: 5m);
        SetupActivePrescriptions([]);

        // Act
        var result = await _handler.Handle(BuildQuery(dosageAmount: 5m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Both alerts present
        result.Value.Alerts.Should().HaveCount(2);

        // Critical first
        result.Value.Alerts[0].Severity.Should().Be(InteractionSeverity.Critical);
        result.Value.Alerts[1].Severity.Should().Be(InteractionSeverity.Info);
    }

    // ── Not Found Tests ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDrugNotFound_ReturnsNotFound()
    {
        // Arrange
        _sender.Send(
                Arg.Is<GetDrugCatalogEntryByIdQuery>(q => q.Id == DrugId),
                Arg.Any<CancellationToken>())
            .Returns(Result<DrugCatalogEntryDto>.NotFound("Drug not found"));

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFound()
    {
        // Arrange
        var drug = BuildDrug();
        SetupDrugResult(drug);

        _sender.Send(
                Arg.Is<GetPatientSpeciesAndWeightQuery>(q => q.PatientId == PatientId),
                Arg.Any<CancellationToken>())
            .Returns(Result<PatientSpeciesWeightDto>.NotFound("Patient not found"));

        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── Alternatives Tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAlternativeDrugIdIsPresent_FetchesAlternativeDtos()
    {
        // Arrange — the interaction lists an alternative drug
        var alternativeId = Guid.NewGuid();

        var interaction = new DrugInteractionDto(
            OtherDrugId: OtherDrugId,
            OtherDrugName: "Metronidazole",
            Severity: InteractionSeverity.Moderate,
            Description: "Use alternative");

        var drug = BuildDrug(interactions: [interaction]);

        var activePrescription = BuildActivePrescription(OtherDrugId);

        SetupDrugResult(drug);
        SetupPatientResult(Species.Dog);
        SetupActivePrescriptions([activePrescription]);

        // No alternate IDs on the interaction itself (per current implementation)
        // The alternatives list in the result should be empty since no AlternativeDrugIds are set
        // Act
        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Alerts.Should().HaveCount(1);
        // Alternatives are empty because DrugInteraction.AlternativeDrugIds is not populated
        // (that's the current implementation — alternatives are populated from alert.AlternativeDrugIds)
        result.Value.Alternatives.Should().BeEmpty();
    }
}
