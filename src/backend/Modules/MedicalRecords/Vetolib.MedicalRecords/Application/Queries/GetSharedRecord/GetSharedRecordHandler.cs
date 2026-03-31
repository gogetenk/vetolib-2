using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.GetSharedRecord;

internal class GetSharedRecordHandler : IRequestHandler<GetSharedRecordQuery, Result<PatientSummaryDto>>
{
    private readonly MedicalRecordsDbContext _context;
    private readonly ISender _sender;

    public GetSharedRecordHandler(MedicalRecordsDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<Result<PatientSummaryDto>> Handle(GetSharedRecordQuery query, CancellationToken ct)
    {
        // Find the link by token — IgnoreQueryFilters because this is a public endpoint
        // and the caller has no tenant context
        var link = await _context.SharedRecordLinks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Token == query.Token, ct);

        if (link is null)
            return Result<PatientSummaryDto>.NotFound("Share link not found.");

        if (!link.IsActive())
            return Result<PatientSummaryDto>.Error("This share link has expired or been revoked.");

        // Increment access count
        link.IncrementAccessCount();
        await _context.SaveChangesAsync(ct);

        // Build the medical summary directly (same logic as GetPatientSummary but no internal notes)
        var patient = await _context.Patients
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .FirstOrDefaultAsync(p => p.Id == link.PatientId && p.ClinicId == link.ClinicId, ct);

        if (patient is null)
            return Result<PatientSummaryDto>.NotFound("Patient not found.");

        // Latest weight
        var latestWeight = await _context.WeightEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(w => w.PatientId == link.PatientId && w.ClinicId == link.ClinicId)
            .OrderByDescending(w => w.RecordedAt)
            .Select(w => (decimal?)w.WeightKg)
            .FirstOrDefaultAsync(ct);

        var weightKg = latestWeight ?? patient.WeightKg;

        // Recent medical records (last 10) — filter out internal notes (Diagnosis starting with "INTERNAL:")
        var recentRecords = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == link.PatientId && r.ClinicId == link.ClinicId)
            .Where(r => !r.Diagnosis.StartsWith("INTERNAL:"))
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

        // Active prescriptions from last 90 days
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

        // Vaccinations
        var vaccinations = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == link.PatientId && r.ClinicId == link.ClinicId)
            .Where(r => r.Treatment.StartsWith("VACCINE:"))
            .OrderByDescending(r => r.ExaminedAt)
            .Take(100)
            .Select(r => new PatientSummaryVaccinationDto(
                r.Treatment.Substring(8).Trim(),
                r.ExaminedAt,
                r.VetName))
            .ToListAsync(ct);

        // Health alerts (allergies) — no internal notes
        var healthAlerts = await _context.MedicalRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.PatientId == link.PatientId && r.ClinicId == link.ClinicId)
            .Where(r => r.Diagnosis.StartsWith("ALLERGY:"))
            .Select(r => r.Diagnosis.Substring(8).Trim())
            .Distinct()
            .Take(100)
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
