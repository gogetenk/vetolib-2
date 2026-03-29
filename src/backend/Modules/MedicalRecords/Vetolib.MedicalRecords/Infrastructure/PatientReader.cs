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
            .Include(p => p.MedicalRecords)
                .ThenInclude(r => r.Prescriptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result<PatientContextDto>.NotFound($"Patient {patientId} not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageYears = today.Year - patient.BirthDate.Year;
        if (patient.BirthDate > today.AddYears(-ageYears)) ageYears--;

        var lastRecord = patient.MedicalRecords
            .OrderByDescending(r => r.ExaminedAt)
            .FirstOrDefault();

        IReadOnlyList<string>? activeMedications = null;
        IReadOnlyList<string>? knownAllergies = null;
        IReadOnlyList<string>? vaccinationHistory = null;

        if (includeFullMedicalContext)
        {
            activeMedications = patient.MedicalRecords
                .SelectMany(r => r.Prescriptions)
                .Select(p => $"{p.Medication} ({p.Dosage})")
                .Distinct()
                .ToList();

            // Allergies and vaccinations are extracted from diagnosis/treatment notes
            // using a simple keyword convention (prefix "ALLERGY:" or "VACCINE:")
            knownAllergies = patient.MedicalRecords
                .Where(r => r.Diagnosis.StartsWith("ALLERGY:", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Diagnosis["ALLERGY:".Length..].Trim())
                .Distinct()
                .ToList();

            vaccinationHistory = patient.MedicalRecords
                .Where(r => r.Treatment.StartsWith("VACCINE:", StringComparison.OrdinalIgnoreCase))
                .Select(r => $"{r.Treatment["VACCINE:".Length..].Trim()} ({r.ExaminedAt:yyyy-MM-dd})")
                .ToList();
        }

        var context = new PatientContextDto(
            patient.Id,
            patient.Name,
            patient.Species.ToString(),
            ageYears,
            lastRecord?.ExaminedAt,
            activeMedications,
            knownAllergies,
            vaccinationHistory);

        return Result<PatientContextDto>.Success(context);
    }

    public async Task<Result<Sex>> GetPatientSexAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result<Sex>.NotFound($"Patient {patientId} not found");

        return Result<Sex>.Success(patient.Sex);
    }
}
