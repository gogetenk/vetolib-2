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
///
/// SAFETY: Drug interaction checks are NEVER gated by preferences (AIDrugInteractions preference
/// is always-on by PO decision for patient safety). Do NOT add an IPreferenceChecker check here.
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
        var drugResult = await _sender.Send(
            new GetDrugCatalogEntryByIdQuery(request.DrugCatalogEntryId), cancellationToken);
        if (!drugResult.IsSuccess)
        {
            _logger.LogWarning("Drug catalog entry {DrugId} not found during interaction check.", request.DrugCatalogEntryId);
            return Result<InteractionCheckResult>.NotFound($"Drug catalog entry {request.DrugCatalogEntryId} not found");
        }

        var patientResult = await _sender.Send(
            new GetPatientSpeciesAndWeightQuery(request.PatientId), cancellationToken);
        if (!patientResult.IsSuccess)
        {
            _logger.LogWarning("Patient {PatientId} not found during interaction check.", request.PatientId);
            return Result<InteractionCheckResult>.NotFound($"Patient {request.PatientId} not found");
        }

        var activePrescriptions = await LoadActivePrescriptionsAsync(request.PatientId, cancellationToken);
        var alerts = await DetectInteractionsAsync(drugResult.Value, patientResult.Value, activePrescriptions, request.DosageAmount, cancellationToken);
        var alternatives = await FetchAlternativeDrugsAsync(alerts, cancellationToken);

        return Result<InteractionCheckResult>.Success(new InteractionCheckResult(alerts, alternatives));
    }

    private async Task<List<PrescriptionDto>> LoadActivePrescriptionsAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var activeDaysWindow = _configuration.GetValue<int>("AI:ActivePrescriptionWindowDays", 90);
        var result = await _sender.Send(
            new GetActivePrescriptionsForPatientQuery(patientId, activeDaysWindow), cancellationToken);
        return result.IsSuccess ? result.Value : new List<PrescriptionDto>();
    }

    private async Task<List<InteractionAlert>> DetectInteractionsAsync(
        DrugCatalogEntryDto drug,
        PatientSpeciesWeightDto patient,
        List<PrescriptionDto> activePrescriptions,
        decimal? dosageAmount,
        CancellationToken cancellationToken)
    {
        var alerts = new List<InteractionAlert>();

        CheckSpeciesContraindications(drug, patient.Species, alerts);
        CheckDrugInteractions(drug, activePrescriptions, alerts);
        await CheckReverseDrugInteractions(drug, activePrescriptions, alerts, cancellationToken);

        if (patient.WeightKg.HasValue && dosageAmount.HasValue)
            CheckDosageOutOfRange(drug, patient.Species, patient.WeightKg.Value, dosageAmount.Value, alerts);

        // Sort by severity: Critical > Moderate > Info
        alerts.Sort((a, b) => a.Severity.CompareTo(b.Severity));

        return alerts;
    }

    private async Task<List<DrugCatalogEntryDto>> FetchAlternativeDrugsAsync(
        List<InteractionAlert> alerts,
        CancellationToken cancellationToken)
    {
        var alternativeDrugIds = alerts
            .SelectMany(a => a.AlternativeDrugIds)
            .Distinct();

        // Failures are best-effort — alternatives are informational only
        var alternatives = new List<DrugCatalogEntryDto>();
        foreach (var altId in alternativeDrugIds)
        {
            var altResult = await _sender.Send(new GetDrugCatalogEntryByIdQuery(altId), cancellationToken);
            if (altResult.IsSuccess)
                alternatives.Add(altResult.Value);
        }

        return alternatives;
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
            if (contraindication.AlternativeDrugId.HasValue)
                alternativeDrugIds.Add(contraindication.AlternativeDrugId.Value);

            alerts.Add(new InteractionAlert(
                Severity: contraindication.Severity,
                Type: InteractionAlertType.SpeciesContraindication,
                Message: $"{drug.DisplayName} is contraindicated for {patientSpecies}: {contraindication.Reason}",
                AlternativeDrugIds: alternativeDrugIds));
        }
    }

    private async Task CheckReverseDrugInteractions(
        DrugCatalogEntryDto newDrug,
        List<PrescriptionDto> activePrescriptions,
        List<InteractionAlert> alerts,
        CancellationToken cancellationToken)
    {
        // For each active prescription that references a catalog entry,
        // fetch that entry and check if it lists an interaction with the new drug.
        var activeCatalogEntryIds = activePrescriptions
            .Where(p => p.DrugCatalogEntryId.HasValue)
            .Select(p => p.DrugCatalogEntryId!.Value)
            .Distinct();

        foreach (var existingDrugId in activeCatalogEntryIds)
        {
            // Skip if we already caught this via forward check
            if (newDrug.Interactions.Any(i => i.OtherDrugId == existingDrugId))
                continue;

            var existingDrugResult = await _sender.Send(
                new GetDrugCatalogEntryByIdQuery(existingDrugId), cancellationToken);

            if (!existingDrugResult.IsSuccess)
                continue;

            var existingDrug = existingDrugResult.Value;
            var reverseInteraction = existingDrug.Interactions
                .FirstOrDefault(i => i.OtherDrugId == newDrug.Id);

            if (reverseInteraction is null)
                continue;

            // Avoid duplicate alerts
            var alreadyAlerted = alerts.Any(a =>
                a.Type == InteractionAlertType.DrugInteraction &&
                a.Message.Contains(existingDrug.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                a.Message.Contains(newDrug.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (!alreadyAlerted)
            {
                alerts.Add(new InteractionAlert(
                    Severity: reverseInteraction.Severity,
                    Type: InteractionAlertType.DrugInteraction,
                    Message: $"{newDrug.DisplayName} interacts with {existingDrug.DisplayName}: {reverseInteraction.Description}",
                    AlternativeDrugIds: new List<Guid>()));
            }
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
