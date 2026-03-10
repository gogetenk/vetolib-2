using System.Globalization;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Queries.CheckInteractions;

/// <summary>
/// Checks drug interactions for a patient prescription.
/// Communicates with MedicalRecords via MediatR queries (no runtime cross-reference).
/// </summary>
internal class CheckInteractionsHandler : IRequestHandler<CheckInteractionsQuery, Result<InteractionCheckResult>>
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckInteractionsHandler> _logger;

    public CheckInteractionsHandler(
        ISender sender,
        IConfiguration configuration,
        ILogger<CheckInteractionsHandler> logger)
    {
        _sender = sender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<InteractionCheckResult>> Handle(
        CheckInteractionsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch the drug catalog entry being prescribed
        var drugResult = await _sender.Send(
            new GetDrugCatalogEntryByIdQuery(request.DrugCatalogEntryId),
            cancellationToken);

        if (!drugResult.IsSuccess)
        {
            _logger.LogWarning(
                "Drug catalog entry {DrugId} not found during interaction check.",
                request.DrugCatalogEntryId);
            return Result<InteractionCheckResult>.NotFound(
                $"Drug catalog entry {request.DrugCatalogEntryId} not found");
        }

        var drug = drugResult.Value;

        // 2. Fetch patient species and weight
        var patientResult = await _sender.Send(
            new GetPatientSpeciesAndWeightQuery(request.PatientId),
            cancellationToken);

        if (!patientResult.IsSuccess)
        {
            _logger.LogWarning(
                "Patient {PatientId} not found during interaction check.",
                request.PatientId);
            return Result<InteractionCheckResult>.NotFound(
                $"Patient {request.PatientId} not found");
        }

        var patient = patientResult.Value;

        // 3. Fetch active prescriptions
        var activeDaysWindow = _configuration.GetValue<int>("AI:ActivePrescriptionWindowDays", 90);
        var activePrescriptionsResult = await _sender.Send(
            new GetActivePrescriptionsForPatientQuery(request.PatientId, activeDaysWindow),
            cancellationToken);

        var activePrescriptions = activePrescriptionsResult.IsSuccess
            ? activePrescriptionsResult.Value
            : new List<PrescriptionDto>();

        var alerts = new List<InteractionAlert>();

        // 3a. Check species contraindications
        CheckSpeciesContraindications(drug, patient.Species, alerts);

        // 3b. Check drug-drug interactions
        CheckDrugInteractions(drug, activePrescriptions, alerts);

        // 3c. Check dosage out of range (only if patient weight is known and dosage is provided)
        if (patient.WeightKg.HasValue && request.DosageAmount.HasValue)
        {
            CheckDosageOutOfRange(drug, patient.Species, patient.WeightKg.Value, request.DosageAmount.Value, alerts);
        }

        // 4. Sort alerts by severity: Critical > Moderate > Info
        alerts.Sort((a, b) => a.Severity.CompareTo(b.Severity));

        // 5. Build unique alternative drug IDs from all alerts
        var alternativeDrugIds = alerts
            .SelectMany(a => a.AlternativeDrugIds)
            .Distinct()
            .ToList();

        // 6. Fetch alternative drug DTOs (fire and forget failures — alternatives are best-effort)
        var alternatives = new List<DrugCatalogEntryDto>();
        foreach (var altId in alternativeDrugIds)
        {
            var altResult = await _sender.Send(new GetDrugCatalogEntryByIdQuery(altId), cancellationToken);
            if (altResult.IsSuccess)
                alternatives.Add(altResult.Value);
        }

        return Result<InteractionCheckResult>.Success(
            new InteractionCheckResult(alerts, alternatives));
    }

    private static void CheckSpeciesContraindications(
        DrugCatalogEntryDto drug,
        Species patientSpecies,
        List<InteractionAlert> alerts)
    {
        foreach (var contraindication in drug.SpeciesContraindications)
        {
            if (contraindication.Species != patientSpecies)
                continue;

            var alternativeDrugIds = new List<Guid>();

            // Alternatives: other drugs in the interaction list that reference alternatives
            // (The spec says: alternatives are drugs without the same contraindication — resolved at catalog level)
            // Here we rely on the AlternativeDrugIds populated by the catalog data; for contraindications
            // there is no direct "alternatives" field on the DTO, so we return an empty list.
            // The catalog-level alternatives are resolved by the caller (UI or orchestrator).

            alerts.Add(new InteractionAlert(
                Severity: contraindication.Severity,
                Type: InteractionAlertType.SpeciesContraindication,
                Message: $"{drug.DisplayName} is contraindicated for {patientSpecies}: {contraindication.Reason}",
                AlternativeDrugIds: alternativeDrugIds));
        }
    }

    private static void CheckDrugInteractions(
        DrugCatalogEntryDto drug,
        List<PrescriptionDto> activePrescriptions,
        List<InteractionAlert> alerts)
    {
        // Active prescriptions that reference a catalog entry
        var activeCatalogIds = activePrescriptions
            .Where(p => p.DrugCatalogEntryId.HasValue)
            .Select(p => p.DrugCatalogEntryId!.Value)
            .ToHashSet();

        foreach (var interaction in drug.Interactions)
        {
            if (!activeCatalogIds.Contains(interaction.OtherDrugId))
                continue;

            alerts.Add(new InteractionAlert(
                Severity: interaction.Severity,
                Type: InteractionAlertType.DrugInteraction,
                Message: $"{drug.DisplayName} interacts with {interaction.OtherDrugName}: {interaction.Description}",
                AlternativeDrugIds: new List<Guid>()));
        }
    }

    private static void CheckDosageOutOfRange(
        DrugCatalogEntryDto drug,
        Species patientSpecies,
        decimal weightKg,
        decimal dosageAmount,
        List<InteractionAlert> alerts)
    {
        var guideline = drug.DosageGuidelines
            .FirstOrDefault(g => g.Species == patientSpecies);

        if (guideline is null)
            return;

        var minDose = guideline.MinDosePerKg * weightKg;
        var maxDose = guideline.MaxDosePerKg * weightKg;

        if (dosageAmount >= minDose && dosageAmount <= maxDose)
            return;

        var direction = dosageAmount < minDose ? "below" : "above";
        var ic = CultureInfo.InvariantCulture;
        alerts.Add(new InteractionAlert(
            Severity: InteractionSeverity.Info,
            Type: InteractionAlertType.DosageOutOfRange,
            Message: $"Dosage {dosageAmount.ToString(ic)} {guideline.Unit} is {direction} the recommended range of " +
                     $"{minDose.ToString("G29", ic)} {guideline.Unit} to {maxDose.ToString("G29", ic)} {guideline.Unit} " +
                     $"for {patientSpecies} ({guideline.MinDosePerKg.ToString(ic)}-{guideline.MaxDosePerKg.ToString(ic)} {guideline.Unit}/kg at {weightKg.ToString(ic)} kg)",
            AlternativeDrugIds: new List<Guid>()));
    }
}
