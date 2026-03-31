using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientReader : IPatientReader
{
    private readonly MedicalRecordsDbContext _context;

    public PatientReader(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientDto>> GetPatientByIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result<PatientDto>.NotFound($"Patient {patientId} not found");

        return Result<PatientDto>.Success(patient.ToDto());
    }

    public async Task<Result<IReadOnlyList<PatientDto>>> GetPatientsByOwnerIdAsync(
        Guid ownerId,
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        // The global query filter already applies ClinicId = current tenant.
        // We additionally filter by OwnerId via the PatientOwner join table.
        var patients = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .Where(p => p.PatientOwners.Any(po => po.OwnerId == ownerId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = patients.Select(p => p.ToDto()).ToList();
        return Result<IReadOnlyList<PatientDto>>.Success(dtos);
    }

    public async Task<Result<PatientContextDto>> GetPatientContextAsync(
        Guid patientId,
        bool includeFullMedicalContext,
        CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result<PatientContextDto>.NotFound($"Patient {patientId} not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageYears = today.Year - patient.BirthDate.Year;
        if (patient.BirthDate > today.AddYears(-ageYears)) ageYears--;

        DateTime? lastExaminedAt;
        IReadOnlyList<string>? activeMedications = null;
        IReadOnlyList<string>? knownAllergies = null;
        IReadOnlyList<string>? vaccinationHistory = null;

        if (includeFullMedicalContext)
        {
            // Load the last 50 medical records with prescriptions for full context
            const int maxRecords = 50;
            var recentRecords = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.ExaminedAt)
                .Take(maxRecords)
                .Include(r => r.Prescriptions)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            lastExaminedAt = recentRecords.FirstOrDefault()?.ExaminedAt;

            activeMedications = recentRecords
                .SelectMany(r => r.Prescriptions)
                .Select(p => $"{p.Medication} ({p.Dosage})")
                .Distinct()
                .ToList();

            // Allergies and vaccinations are extracted from diagnosis/treatment notes
            // using a simple keyword convention (prefix "ALLERGY:" or "VACCINE:")
            knownAllergies = recentRecords
                .Where(r => r.Diagnosis.StartsWith("ALLERGY:", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Diagnosis["ALLERGY:".Length..].Trim())
                .Distinct()
                .ToList();

            vaccinationHistory = recentRecords
                .Where(r => r.Treatment.StartsWith("VACCINE:", StringComparison.OrdinalIgnoreCase))
                .Select(r => $"{r.Treatment["VACCINE:".Length..].Trim()} ({r.ExaminedAt:yyyy-MM-dd})")
                .ToList();
        }
        else
        {
            // Lightweight path: only fetch the most recent ExaminedAt date
            lastExaminedAt = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.ExaminedAt)
                .Select(r => (DateTime?)r.ExaminedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var context = new PatientContextDto(
            patient.Id,
            patient.Name,
            patient.Species.ToString(),
            ageYears,
            lastExaminedAt,
            activeMedications,
            knownAllergies,
            vaccinationHistory);

        return Result<PatientContextDto>.Success(context);
    }

    public async Task<Result<PatientBasicInfoDto>> GetPatientBasicInfoAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result<PatientBasicInfoDto>.NotFound($"Patient {patientId} not found");

        return Result<PatientBasicInfoDto>.Success(
            new PatientBasicInfoDto(patient.Id, patient.Name, patient.Species, patient.Sex));
    }

    public async Task<Result<IReadOnlyDictionary<Guid, PatientDto>>> GetPatientsByIdsAsync(
        IReadOnlyCollection<Guid> patientIds,
        CancellationToken cancellationToken = default)
    {
        if (patientIds.Count == 0)
            return Result<IReadOnlyDictionary<Guid, PatientDto>>.Success(
                new Dictionary<Guid, PatientDto>());

        var patients = await _context.Patients
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .Where(p => patientIds.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dict = patients.ToDictionary(p => p.Id, p => p.ToDto());
        return Result<IReadOnlyDictionary<Guid, PatientDto>>.Success(dict);
    }
}
