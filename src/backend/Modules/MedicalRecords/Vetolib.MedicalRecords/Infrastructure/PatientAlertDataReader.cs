using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientAlertDataReader : IPatientAlertDataReader
{
    private readonly MedicalRecordsDbContext _context;

    public PatientAlertDataReader(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PatientAlertDataDto>>> GetAllActivePatientsWithRecordsAsync(
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-24);

        var patients = await _context.Patients
            .Include(p => p.MedicalRecords.Where(r => r.ExaminedAt >= cutoffDate))
            .Include(p => p.WeightEntries)
            .AsNoTracking()
            .ToListAsync(ct);

        var dtos = patients.Select(p =>
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

            return new PatientAlertDataDto(
                p.Id,
                p.Name,
                p.Species,
                p.Breed,
                p.BirthDate,
                p.WeightKg,
                recentRecords,
                weightHistory);
        }).ToList();

        return Result<IReadOnlyList<PatientAlertDataDto>>.Success(dtos);
    }
}
