using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientAlertDataReader : IPatientAlertDataReader
{
    private const int BatchSize = 100;

    private readonly MedicalRecordsDbContext _context;

    public PatientAlertDataReader(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PatientAlertDataDto>>> GetAllActivePatientsWithRecordsAsync(
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-24);

        // Get total count first to avoid loading all patients at once
        var totalPatients = await _context.Patients.CountAsync(ct);

        var allDtos = new List<PatientAlertDataDto>(totalPatients);

        // Process patients in batches to avoid memory pressure on large clinics
        for (var skip = 0; skip < totalPatients; skip += BatchSize)
        {
            var batch = await _context.Patients
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(BatchSize)
                .Include(p => p.MedicalRecords.Where(r => r.ExaminedAt >= cutoffDate))
                .Include(p => p.WeightEntries)
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (var p in batch)
            {
                var recentRecords = p.MedicalRecords
                    .OrderByDescending(r => r.ExaminedAt)
                    .Select(r => new MedicalRecordSummaryDto(
                        r.Id,
                        r.Diagnosis,
                        r.VetName,
                        r.ExaminedAt))
                    .ToList();

                var weightHistory = p.WeightEntries
                    .OrderByDescending(w => w.RecordedAt)
                    .Select(w => new WeightEntryDto(w.Id, w.PatientId, w.WeightKg, w.RecordedAt, w.RecordedBy, w.Note))
                    .ToList();

                allDtos.Add(new PatientAlertDataDto(
                    p.Id,
                    p.Name,
                    p.Species,
                    p.Breed,
                    p.BirthDate,
                    p.WeightKg,
                    recentRecords,
                    weightHistory));
            }
        }

        return Result<IReadOnlyList<PatientAlertDataDto>>.Success(allDtos);
    }
}
