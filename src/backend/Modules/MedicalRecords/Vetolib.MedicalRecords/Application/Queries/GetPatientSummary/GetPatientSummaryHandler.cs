using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientSummary;

internal class GetPatientSummaryHandler : IRequestHandler<GetPatientSummaryQuery, Result<PatientSummaryDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public GetPatientSummaryHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientSummaryDto>> Handle(GetPatientSummaryQuery query, CancellationToken ct)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == query.PatientId, ct);

        if (patient is null)
            return Result<PatientSummaryDto>.NotFound($"Patient '{query.PatientId}' not found.");

        // Latest weight from WeightEntries
        var latestWeight = await _context.WeightEntries
            .AsNoTracking()
            .Where(w => w.PatientId == query.PatientId)
            .OrderByDescending(w => w.RecordedAt)
            .Select(w => (decimal?)w.WeightKg)
            .FirstOrDefaultAsync(ct);

        // Use patient.WeightKg as fallback if no weight entries exist
        var weightKg = latestWeight ?? patient.WeightKg;

        // Recent medical records (last 10) with prescriptions
        var recentRecords = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .OrderByDescending(r => r.ExaminedAt)
            .Take(10)
            .Include(r => r.Prescriptions)
            .AsSplitQuery()
            .ToListAsync(ct);

        var recentRecordDtos = recentRecords
            .Select(r => new PatientSummaryMedicalRecordDto(
                r.Id,
                r.Diagnosis,
                r.Treatment,
                r.VetName,
                r.ExaminedAt))
            .ToList();

        // Active prescriptions: from records in the last 90 days
        var cutoff = DateTime.UtcNow.AddDays(-90);
        var activePrescriptions = recentRecords
            .Where(r => r.ExaminedAt >= cutoff)
            .SelectMany(r => r.Prescriptions)
            .Select(p => new PatientSummaryPrescriptionDto(
                p.Id,
                p.Medication,
                p.Dosage,
                p.CreatedAt))
            .ToList();

        // Vaccinations: medical records where Treatment starts with "VACCINE:"
        // Load all vaccination records (not just the last 10)
        var vaccinations = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .Where(r => r.Treatment.StartsWith("VACCINE:"))
            .OrderByDescending(r => r.ExaminedAt)
            .Select(r => new PatientSummaryVaccinationDto(
                r.Treatment.Substring(8).Trim(),
                r.ExaminedAt,
                r.VetName))
            .ToListAsync(ct);

        // Health alerts: allergies from diagnosis prefix "ALLERGY:"
        var healthAlerts = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == query.PatientId)
            .Where(r => r.Diagnosis.StartsWith("ALLERGY:"))
            .Select(r => r.Diagnosis.Substring(8).Trim())
            .Distinct()
            .ToListAsync(ct);

        var firstOwner = patient.PatientOwners
            .FirstOrDefault(po => po.Owner is not null)?.Owner;

        var ownerDto = firstOwner is not null
            ? new PatientSummaryOwnerInfoDto(
                $"{firstOwner.FirstName} {firstOwner.LastName}".Trim(),
                firstOwner.Email,
                firstOwner.Phone)
            : null;

        var patientInfoDto = new PatientSummaryPatientInfoDto(
            patient.Id,
            patient.Name,
            patient.Species,
            patient.Breed,
            patient.Sex,
            patient.BirthDate,
            patient.MicrochipNumber,
            weightKg);

        var summary = new PatientSummaryDto(
            patientInfoDto,
            ownerDto,
            recentRecordDtos,
            activePrescriptions,
            vaccinations,
            healthAlerts,
            DateTime.UtcNow);

        return Result<PatientSummaryDto>.Success(summary);
    }
}
