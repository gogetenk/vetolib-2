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

        var medicalRecord = await _context.MedicalRecords
            .FirstOrDefaultAsync(r => r.Id == cmd.MedicalRecordId, ct);

        if (medicalRecord is null)
            return Result<PrescriptionDto>.NotFound("Dossier médical introuvable");

        // Check drug interactions when a catalog entry is specified
        InteractionSeverity? highestCriticalSeverity = null;
        if (cmd.DrugCatalogEntryId.HasValue)
        {
            var interactionQuery = new CheckInteractionsQuery(
                cmd.PatientId,
                cmd.DrugCatalogEntryId.Value,
                cmd.DosageAmount,
                cmd.ClinicId);

            var interactionResult = await _sender.Send(interactionQuery, ct);

            if (interactionResult.IsSuccess)
            {
                var criticalAlerts = interactionResult.Value.Alerts
                    .Where(a => a.Severity == InteractionSeverity.Critical)
                    .ToList();

                if (criticalAlerts.Count > 0)
                {
                    var justification = cmd.OverrideJustification?.Trim();
                    if (string.IsNullOrWhiteSpace(justification) || justification.Length < 10)
                    {
                        return Result<PrescriptionDto>.Error(
                            "Override justification required for critical alerts");
                    }

                    // Override is valid — record the highest severity for the override trail
                    highestCriticalSeverity = interactionResult.Value.Alerts
                        .OrderBy(a => a.Severity)
                        .Select(a => a.Severity)
                        .First();
                }
            }
        }

        var prescriptionResult = Prescription.Create(
            cmd.ClinicId,
            cmd.MedicalRecordId,
            cmd.Medication,
            cmd.Dosage,
            cmd.VetLicenseNumber,
            cmd.DrugCatalogEntryId);

        if (!prescriptionResult.IsSuccess)
            return Result<PrescriptionDto>.Invalid(prescriptionResult.ValidationErrors.ToList());

        var prescription = prescriptionResult.Value;

        // Apply override stamp if critical alerts were present and justification provided
        if (highestCriticalSeverity.HasValue && !string.IsNullOrWhiteSpace(cmd.OverrideJustification))
        {
            var overrideResult = prescription.ApplyOverride(cmd.OverrideJustification, highestCriticalSeverity.Value);
            if (!overrideResult.IsSuccess)
                return Result<PrescriptionDto>.Error(overrideResult.Errors.First());
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(ct);

        // Publish audit event if an override was applied
        if (!string.IsNullOrWhiteSpace(prescription.OverrideJustification))
        {
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

        return Result<PrescriptionDto>.Success(prescription.ToDto());
    }
}
