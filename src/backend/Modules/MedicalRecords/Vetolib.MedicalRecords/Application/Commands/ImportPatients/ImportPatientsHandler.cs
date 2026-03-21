using Ardalis.Result;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatients;

internal class ImportPatientsHandler : IRequestHandler<ImportPatientsCommand, Result<ImportReportDto>>
{
    private readonly MedicalRecordsDbContext _context;
    private readonly IOutputCacheStore? _cache;

    public ImportPatientsHandler(MedicalRecordsDbContext context, IOutputCacheStore? cache = null)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<ImportReportDto>> Handle(ImportPatientsCommand cmd, CancellationToken ct)
    {
        var errors = new List<string>();
        int imported = 0;
        int skipped = 0;

        List<CsvPatientRow> rows;
        try
        {
            rows = ParseCsv(cmd.CsvStream);
        }
        catch (Exception ex)
        {
            return Result<ImportReportDto>.Error($"CSV_PARSE_ERROR:{ex.Message}");
        }

        if (rows.Count == 0)
        {
            return Result<ImportReportDto>.Success(new ImportReportDto(0, 0, []));
        }

        // Extract emails from CSV first, then load only matching owners (filter before load)
        var csvEmails = rows
            .Select(r => r.OwnerEmail?.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Also include generated emails for rows without explicit email
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.OwnerEmail) && !string.IsNullOrWhiteSpace(row.OwnerName))
            {
                csvEmails.Add(GenerateEmailFromName(row.OwnerName.Trim(), cmd.ClinicId));
            }
        }

        var existingOwnersByEmail = await _context.Owners
            .Where(o => csvEmails.Contains(o.Email))
            .ToDictionaryAsync(o => o.Email, o => o, StringComparer.OrdinalIgnoreCase, ct);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int rowNumber = i + 2; // 1-based + header row

            // Validate required fields
            var rowErrors = ValidateRow(row, rowNumber);
            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors);
                skipped++;
                continue;
            }

            // Parse species
            if (!Enum.TryParse<Species>(row.Species.Trim(), ignoreCase: true, out var species))
            {
                errors.Add($"Row {rowNumber}: unknown species '{row.Species}'");
                skipped++;
                continue;
            }

            // Parse date
            if (!DateOnly.TryParse(row.DateOfBirth.Trim(), out var birthDate))
            {
                errors.Add($"Row {rowNumber}: invalid date of birth '{row.DateOfBirth}' (expected YYYY-MM-DD)");
                skipped++;
                continue;
            }

            // Create or reuse owner
            Owner owner;
            var ownerEmail = row.OwnerEmail.Trim();

            if (!string.IsNullOrWhiteSpace(ownerEmail))
            {
                // Match by email
                if (existingOwnersByEmail.TryGetValue(ownerEmail, out var existingOwner))
                {
                    owner = existingOwner;
                }
                else
                {
                    // Create new owner
                    var nameParts = row.OwnerName.Trim().Split(' ', 2);
                    var firstName = nameParts[0];
                    var lastName = nameParts.Length > 1 ? nameParts[1] : "-";
                    var ownerResult = Owner.Create(cmd.ClinicId, firstName, lastName, ownerEmail, row.OwnerPhone.Trim().NullIfEmpty());
                    if (!ownerResult.IsSuccess)
                    {
                        errors.Add($"Row {rowNumber}: invalid owner data — {string.Join(", ", ownerResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                        skipped++;
                        continue;
                    }
                    owner = ownerResult.Value;
                    _context.Owners.Add(owner);
                    existingOwnersByEmail[ownerEmail] = owner;
                }
            }
            else
            {
                // No email — match by name within existing owners (best-effort) or create with generated email
                var ownerName = row.OwnerName.Trim();
                var generatedEmail = GenerateEmailFromName(ownerName, cmd.ClinicId);

                if (existingOwnersByEmail.TryGetValue(generatedEmail, out var existingByGenEmail))
                {
                    owner = existingByGenEmail;
                }
                else
                {
                    var nameParts = ownerName.Split(' ', 2);
                    var firstName = nameParts[0];
                    var lastName = nameParts.Length > 1 ? nameParts[1] : "-";
                    var ownerResult = Owner.Create(cmd.ClinicId, firstName, lastName, generatedEmail, row.OwnerPhone.Trim().NullIfEmpty());
                    if (!ownerResult.IsSuccess)
                    {
                        errors.Add($"Row {rowNumber}: invalid owner data — {string.Join(", ", ownerResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                        skipped++;
                        continue;
                    }
                    owner = ownerResult.Value;
                    _context.Owners.Add(owner);
                    existingOwnersByEmail[generatedEmail] = owner;
                }
            }

            // Create patient
            var patientResult = Patient.Create(cmd.ClinicId, row.PatientName.Trim(), species, row.Breed.Trim(), birthDate);
            if (!patientResult.IsSuccess)
            {
                errors.Add($"Row {rowNumber}: invalid patient data — {string.Join(", ", patientResult.ValidationErrors.Select(e => e.ErrorMessage))}");
                skipped++;
                continue;
            }

            var patient = patientResult.Value;

            // Link owner to patient
            var patientOwner = PatientOwner.Create(cmd.ClinicId, patient.Id, owner.Id);
            patient.AddOwner(patientOwner);
            _context.Patients.Add(patient);
            imported++;
        }

        if (imported > 0)
        {
            await _context.SaveChangesAsync(ct);
            if (_cache is not null) await _cache.EvictByTagAsync("patients", ct);
            if (_cache is not null) await _cache.EvictByTagAsync("dashboard", ct);
        }

        return Result<ImportReportDto>.Success(new ImportReportDto(imported, skipped, errors.AsReadOnly()));
    }

    private static List<CsvPatientRow> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
        };
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<CsvPatientRow>().ToList();
    }

    private static List<string> ValidateRow(CsvPatientRow row, int rowNumber)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.PatientName))
            errors.Add($"Row {rowNumber}: PatientName is required");

        if (string.IsNullOrWhiteSpace(row.Species))
            errors.Add($"Row {rowNumber}: Species is required");

        if (string.IsNullOrWhiteSpace(row.OwnerName))
            errors.Add($"Row {rowNumber}: OwnerName is required");

        return errors;
    }

    private static string GenerateEmailFromName(string ownerName, Guid clinicId)
    {
        var normalized = ownerName.ToLowerInvariant()
            .Replace(" ", ".")
            .Replace("'", "")
            .Replace("-", "");
        var shortId = clinicId.ToString("N")[..8];
        return $"{normalized}.{shortId}@import.vetoclinic.ae";
    }
}

internal static class StringExtensions
{
    internal static string? NullIfEmpty(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
