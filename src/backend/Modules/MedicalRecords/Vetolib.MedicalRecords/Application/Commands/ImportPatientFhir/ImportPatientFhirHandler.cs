using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;

internal class ImportPatientFhirHandler : IRequestHandler<ImportPatientFhirCommand, Result<FhirImportResultDto>>
{
    private readonly MedicalRecordsDbContext _context;

    public ImportPatientFhirHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FhirImportResultDto>> Handle(ImportPatientFhirCommand cmd, CancellationToken ct)
    {
        // 1. Parse the FHIR Bundle
        var parseResult = FhirR4BundleParser.Parse(cmd.FhirBundleJson);
        if (!parseResult.IsSuccess)
            return Result<FhirImportResultDto>.Invalid(parseResult.ValidationErrors.ToList());

        var bundle = parseResult.Value;
        var warnings = new List<string>();

        // 2. Deduplication: check if patient with same microchip already exists
        Patient patient;
        bool wasMerged = false;

        if (!string.IsNullOrWhiteSpace(bundle.Patient.MicrochipNumber))
        {
            var existing = await _context.Patients
                .FirstOrDefaultAsync(p => p.MicrochipNumber == bundle.Patient.MicrochipNumber, ct);

            if (existing is not null)
            {
                patient = existing;
                wasMerged = true;
                warnings.Add($"Patient with microchip '{bundle.Patient.MicrochipNumber}' already exists — merging records into existing patient '{existing.Name}' ({existing.Id})");
            }
            else
            {
                var createResult = CreatePatientFromFhir(cmd.ClinicId, bundle.Patient);
                if (!createResult.IsSuccess)
                    return Result<FhirImportResultDto>.Invalid(createResult.ValidationErrors.ToList());

                patient = createResult.Value;
                _context.Patients.Add(patient);
            }
        }
        else
        {
            var createResult = CreatePatientFromFhir(cmd.ClinicId, bundle.Patient);
            if (!createResult.IsSuccess)
                return Result<FhirImportResultDto>.Invalid(createResult.ValidationErrors.ToList());

            patient = createResult.Value;
            _context.Patients.Add(patient);
            warnings.Add("No microchip number in FHIR Patient — deduplication not possible, new patient created");
        }

        // 3. Import Encounters as MedicalRecords
        // Build a map from FHIR encounter ID to the created MedicalRecord ID for prescription linking
        var encounterIdMap = new Dictionary<string, Guid>();
        int medicalRecordsImported = 0;

        foreach (var encounter in bundle.Encounters)
        {
            var recordResult = MedicalRecord.Create(
                cmd.ClinicId,
                patient.Id,
                encounter.Diagnosis,
                encounter.Treatment,
                encounter.VetName,
                encounter.ExaminedAt);

            if (recordResult.IsSuccess)
            {
                var record = recordResult.Value;
                _context.MedicalRecords.Add(record);
                encounterIdMap[encounter.FhirId] = record.Id;
                medicalRecordsImported++;
            }
            else
            {
                warnings.Add($"Skipped encounter '{encounter.FhirId}': {string.Join(", ", recordResult.ValidationErrors.Select(e => e.ErrorMessage))}");
            }
        }

        // 4. Import MedicationRequests as Prescriptions
        int prescriptionsImported = 0;

        foreach (var medReq in bundle.MedicationRequests)
        {
            if (!encounterIdMap.TryGetValue(medReq.EncounterFhirId, out var medicalRecordId))
            {
                // If encounter not found but we have records, attach to the first one
                if (encounterIdMap.Count > 0)
                {
                    medicalRecordId = encounterIdMap.Values.First();
                    warnings.Add($"MedicationRequest for encounter '{medReq.EncounterFhirId}' not found — attached to first imported record");
                }
                else
                {
                    warnings.Add($"Skipped MedicationRequest for '{medReq.Medication}': no matching encounter found");
                    continue;
                }
            }

            var prescriptionResult = Prescription.Create(
                cmd.ClinicId,
                medicalRecordId,
                medReq.Medication,
                medReq.Dosage,
                medReq.VetLicenseNumber);

            if (prescriptionResult.IsSuccess)
            {
                _context.Prescriptions.Add(prescriptionResult.Value);
                prescriptionsImported++;
            }
            else
            {
                warnings.Add($"Skipped prescription for '{medReq.Medication}': {string.Join(", ", prescriptionResult.ValidationErrors.Select(e => e.ErrorMessage))}");
            }
        }

        // 5. Import Observations as WeightEntries
        int weightEntriesImported = 0;

        foreach (var obs in bundle.WeightObservations)
        {
            var weightResult = WeightEntry.Create(
                cmd.ClinicId,
                patient.Id,
                obs.WeightKg,
                "FHIR Import",
                obs.Note,
                obs.RecordedAt);

            if (weightResult.IsSuccess)
            {
                _context.WeightEntries.Add(weightResult.Value);
                weightEntriesImported++;
            }
            else
            {
                warnings.Add($"Skipped weight entry ({obs.WeightKg}kg): {string.Join(", ", weightResult.ValidationErrors.Select(e => e.ErrorMessage))}");
            }
        }

        // 6. Save everything
        await _context.SaveChangesAsync(ct);

        return Result<FhirImportResultDto>.Success(new FhirImportResultDto(
            patient.Id,
            patient.Name,
            wasMerged,
            medicalRecordsImported,
            prescriptionsImported,
            weightEntriesImported,
            warnings));
    }

    private static Result<Patient> CreatePatientFromFhir(Guid clinicId, FhirR4BundleParser.FhirPatientData data)
    {
        return Patient.Create(
            clinicId,
            data.Name,
            data.Species,
            data.Breed,
            data.BirthDate,
            data.Sex,
            data.MicrochipNumber);
    }
}
