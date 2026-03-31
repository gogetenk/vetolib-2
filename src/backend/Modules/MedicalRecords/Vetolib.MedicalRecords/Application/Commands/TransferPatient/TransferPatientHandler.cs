using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.TransferPatient;

internal class TransferPatientHandler : IRequestHandler<TransferPatientCommand, Result<TransferPatientResultDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public TransferPatientHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransferPatientResultDto>> Handle(TransferPatientCommand cmd, CancellationToken ct)
    {
        var warnings = new List<string>();

        // 1. Load source patient (current tenant context ensures ownership)
        var sourcePatient = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == cmd.PatientId, ct);

        if (sourcePatient is null)
            return Result<TransferPatientResultDto>.NotFound($"Patient '{cmd.PatientId}' not found.");

        if (sourcePatient.IsTransferred)
            return Result<TransferPatientResultDto>.Error($"Patient '{cmd.PatientId}' has already been transferred.");

        // 2. Validate target clinic exists by checking if any patient exists there
        //    (We cannot query Auth module directly — use IgnoreQueryFilters to check cross-clinic)
        //    Note: For a real multi-service architecture, a dedicated clinic validation would be better.
        //    For now, we trust the caller (VetOrAdmin) provides a valid clinic ID.
        if (cmd.TargetClinicId == cmd.SourceClinicId)
            return Result<TransferPatientResultDto>.Error("Cannot transfer a patient to the same clinic.");

        // 3. Create TransferLog
        var transferLogResult = TransferLog.Create(
            cmd.SourceClinicId,
            cmd.TargetClinicId,
            cmd.PatientId,
            cmd.TransferredBy);

        if (!transferLogResult.IsSuccess)
            return Result<TransferPatientResultDto>.Invalid(transferLogResult.ValidationErrors.ToList());

        var transferLog = transferLogResult.Value;
        _context.TransferLogs.Add(transferLog);

        try
        {
            // 4. Create new patient in target clinic
            var newPatientResult = Patient.Create(
                cmd.TargetClinicId,
                sourcePatient.Name,
                sourcePatient.Species,
                sourcePatient.Breed,
                sourcePatient.BirthDate,
                sourcePatient.Sex,
                sourcePatient.MicrochipNumber);

            if (!newPatientResult.IsSuccess)
            {
                transferLog.MarkFailed("Failed to create target patient: " +
                    string.Join(", ", newPatientResult.ValidationErrors.Select(e => e.ErrorMessage)));
                await _context.SaveChangesAsync(ct);
                return Result<TransferPatientResultDto>.Error("Failed to create patient in target clinic.");
            }

            var newPatient = newPatientResult.Value;

            // Copy weight if set
            if (sourcePatient.WeightKg.HasValue)
                newPatient.SetWeight(sourcePatient.WeightKg.Value);

            _context.Patients.Add(newPatient);

            // 5. Copy owners to target clinic
            foreach (var po in sourcePatient.PatientOwners.Where(po => po.Owner is not null))
            {
                var owner = po.Owner!;

                // Check if owner already exists in target clinic (by email or phone)
                var existingOwner = await _context.Owners
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(o => o.ClinicId == cmd.TargetClinicId &&
                        (o.Email == owner.Email || o.Phone == owner.Phone), ct);

                Guid targetOwnerId;
                if (existingOwner is not null)
                {
                    targetOwnerId = existingOwner.Id;
                    warnings.Add($"Owner '{owner.FirstName} {owner.LastName}' already exists in target clinic — linked to existing owner.");
                }
                else
                {
                    var newOwner = Owner.Create(
                        cmd.TargetClinicId,
                        owner.FirstName,
                        owner.LastName,
                        owner.Email,
                        owner.Phone);

                    if (newOwner.IsSuccess)
                    {
                        _context.Owners.Add(newOwner.Value);
                        targetOwnerId = newOwner.Value.Id;
                    }
                    else
                    {
                        warnings.Add($"Failed to copy owner '{owner.FirstName} {owner.LastName}': {string.Join(", ", newOwner.ValidationErrors.Select(e => e.ErrorMessage))}");
                        continue;
                    }
                }

                var newPo = PatientOwner.Create(cmd.TargetClinicId, newPatient.Id, targetOwnerId);
                newPatient.AddOwner(newPo);
            }

            // 6. Copy medical records + prescriptions if requested
            int medicalRecordsCopied = 0;
            if (cmd.IncludeRecords)
            {
                var records = await _context.MedicalRecords
                    .AsNoTracking()
                    .Where(r => r.PatientId == cmd.PatientId)
                    .Include(r => r.Prescriptions)
                    .AsSplitQuery()
                    .ToListAsync(ct);

                foreach (var record in records)
                {
                    var newRecordResult = MedicalRecord.Create(
                        cmd.TargetClinicId,
                        newPatient.Id,
                        record.Diagnosis,
                        record.Treatment,
                        record.VetName,
                        record.ExaminedAt);

                    if (!newRecordResult.IsSuccess)
                    {
                        warnings.Add($"Skipped medical record from {record.ExaminedAt:yyyy-MM-dd}: {string.Join(", ", newRecordResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                        continue;
                    }

                    var newRecord = newRecordResult.Value;
                    _context.MedicalRecords.Add(newRecord);

                    // Copy prescriptions
                    foreach (var rx in record.Prescriptions)
                    {
                        var newRxResult = Prescription.Create(
                            cmd.TargetClinicId,
                            newRecord.Id,
                            rx.Medication,
                            rx.Dosage,
                            rx.VetLicenseNumber);

                        if (newRxResult.IsSuccess)
                            _context.Prescriptions.Add(newRxResult.Value);
                        else
                            warnings.Add($"Skipped prescription '{rx.Medication}': {string.Join(", ", newRxResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                    }

                    medicalRecordsCopied++;
                }
            }

            // 7. Copy weight history if requested
            int weightEntriesCopied = 0;
            if (cmd.IncludeWeightHistory)
            {
                var weightEntries = await _context.WeightEntries
                    .AsNoTracking()
                    .Where(w => w.PatientId == cmd.PatientId)
                    .ToListAsync(ct);

                foreach (var entry in weightEntries)
                {
                    var newEntryResult = WeightEntry.Create(
                        cmd.TargetClinicId,
                        newPatient.Id,
                        entry.WeightKg,
                        entry.RecordedBy,
                        entry.Note,
                        entry.RecordedAt);

                    if (newEntryResult.IsSuccess)
                    {
                        _context.WeightEntries.Add(newEntryResult.Value);
                        weightEntriesCopied++;
                    }
                    else
                    {
                        warnings.Add($"Skipped weight entry ({entry.WeightKg}kg): {string.Join(", ", newEntryResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            // 8. Mark source patient as transferred (read-only copy remains)
            var markResult = sourcePatient.MarkAsTransferred(cmd.TargetClinicId);
            if (!markResult.IsSuccess)
            {
                transferLog.MarkFailed("Failed to mark source patient as transferred: " + string.Join(", ", markResult.Errors));
                await _context.SaveChangesAsync(ct);
                return Result<TransferPatientResultDto>.Error("Failed to mark patient as transferred.");
            }

            // 9. Complete transfer
            transferLog.MarkCompleted();
            await _context.SaveChangesAsync(ct);

            return Result<TransferPatientResultDto>.Success(new TransferPatientResultDto(
                sourcePatient.Id,
                newPatient.Id,
                cmd.SourceClinicId,
                cmd.TargetClinicId,
                transferLog.TransferredAt,
                medicalRecordsCopied,
                weightEntriesCopied,
                warnings));
        }
        catch (Exception ex)
        {
            transferLog.MarkFailed(ex.Message);
            // Detach all added entities to avoid saving partial state
            foreach (var entry in _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity != transferLog))
            {
                entry.State = EntityState.Detached;
            }
            await _context.SaveChangesAsync(ct);
            return Result<TransferPatientResultDto>.Error($"Transfer failed: {ex.Message}");
        }
    }
}
