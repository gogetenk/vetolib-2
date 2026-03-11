using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal class AddPrescriptionHandler : IRequestHandler<AddPrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly MedicalRecordsDbContext _context;
    private readonly ISender _sender;
    private readonly IPublisher _publisher;

    public AddPrescriptionHandler(MedicalRecordsDbContext context, ISender sender, IPublisher publisher)
    {
        _context = context;
        _sender = sender;
        _publisher = publisher;
    }

    public async Task<Result<PrescriptionDto>> Handle(AddPrescriptionCommand cmd, CancellationToken ct)
    {
        // RBAC: only Vet and Admin can create prescriptions
        if (cmd.UserRole is not ("Vet" or "Admin"))
            return Result<PrescriptionDto>.Forbidden();

        var medicalRecord = await GetMedicalRecordAsync(cmd.MedicalRecordId, ct);
        if (medicalRecord is null)
            return Result<PrescriptionDto>.NotFound("Dossier médical introuvable");

        var interactionCheckResult = await CheckDrugInteractionsAsync(cmd, ct);
        if (!interactionCheckResult.IsSuccess)
            return Result<PrescriptionDto>.Error(interactionCheckResult.Errors.First());

        var highestCriticalSeverity = interactionCheckResult.Value;

        var prescriptionResult = BuildPrescription(cmd);
        if (!prescriptionResult.IsSuccess)
            return Result<PrescriptionDto>.Invalid(prescriptionResult.ValidationErrors.ToList());

        var prescription = prescriptionResult.Value;

        var applyOverrideResult = ApplyOverrideIfNeeded(prescription, cmd.OverrideJustification, highestCriticalSeverity);
        if (!applyOverrideResult.IsSuccess)
            return Result<PrescriptionDto>.Error(applyOverrideResult.Errors.First());

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(ct);

        await PublishOverrideEventIfNeeded(prescription, cmd, ct);

        return Result<PrescriptionDto>.Success(prescription.ToDto());
    }

    private async Task<MedicalRecord?> GetMedicalRecordAsync(Guid medicalRecordId, CancellationToken ct)
        => await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == medicalRecordId, ct);

    private async Task<Result<InteractionSeverity?>> CheckDrugInteractionsAsync(AddPrescriptionCommand cmd, CancellationToken ct)
    {
        if (!cmd.DrugCatalogEntryId.HasValue)
            return Result<InteractionSeverity?>.Success(null);

        var interactionQuery = new CheckInteractionsQuery(
            cmd.PatientId,
            cmd.DrugCatalogEntryId.Value,
            cmd.DosageAmount,
            cmd.ClinicId);

        var interactionResult = await _sender.Send(interactionQuery, ct);

        if (!interactionResult.IsSuccess)
            return Result<InteractionSeverity?>.Success(null);

        var criticalAlerts = interactionResult.Value.Alerts
            .Where(a => a.Severity == InteractionSeverity.Critical)
            .ToList();

        if (criticalAlerts.Count == 0)
            return Result<InteractionSeverity?>.Success(null);

        var justification = cmd.OverrideJustification?.Trim();
        if (string.IsNullOrWhiteSpace(justification) || justification.Length < 10)
            return Result<InteractionSeverity?>.Error("Override justification required for critical alerts");

        // Override is valid — record the highest severity for the override trail
        var highestSeverity = interactionResult.Value.Alerts
            .OrderBy(a => a.Severity)
            .Select(a => a.Severity)
            .First();

        return Result<InteractionSeverity?>.Success(highestSeverity);
    }

    private static Result<Prescription> BuildPrescription(AddPrescriptionCommand cmd)
        => Prescription.Create(
            cmd.ClinicId,
            cmd.MedicalRecordId,
            cmd.Medication,
            cmd.Dosage,
            cmd.VetLicenseNumber,
            cmd.DrugCatalogEntryId);

    private static Result ApplyOverrideIfNeeded(Prescription prescription, string? overrideJustification, InteractionSeverity? highestCriticalSeverity)
    {
        if (!highestCriticalSeverity.HasValue || string.IsNullOrWhiteSpace(overrideJustification))
            return Result.Success();

        return prescription.ApplyOverride(overrideJustification, highestCriticalSeverity.Value);
    }

    private async Task PublishOverrideEventIfNeeded(Prescription prescription, AddPrescriptionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(prescription.OverrideJustification))
            return;

        await _publisher.Publish(new PrescriptionOverriddenEvent(
            PrescriptionId: prescription.Id,
            PatientId: cmd.PatientId,
            ClinicId: cmd.ClinicId,
            VetId: cmd.VetId,
            VetLicenseNumber: cmd.VetLicenseNumber,
            OverrideJustification: prescription.OverrideJustification,
            OverrideSeverity: Enum.Parse<InteractionSeverity>(prescription.OverrideSeverity!),
            DrugCatalogEntryId: cmd.DrugCatalogEntryId,
            Timestamp: DateTime.UtcNow), ct);
    }
}
