using System.Text.RegularExpressions;
using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal partial class Patient : BaseEntity, IMultiTenant, IAggregateRoot
{
    private static readonly Regex MicrochipRegex = MicrochipPattern();

    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Species Species { get; private set; }
    public string Breed { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public Sex Sex { get; private set; } = Sex.Unknown;
    public decimal? WeightKg { get; private set; }
    public string? MicrochipNumber { get; private set; }

    private readonly List<PatientOwner> _patientOwners = [];
    public IReadOnlyList<PatientOwner> PatientOwners => _patientOwners.AsReadOnly();

    private readonly List<MedicalRecord> _medicalRecords = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    private readonly List<WeightEntry> _weightEntries = [];
    public IReadOnlyList<WeightEntry> WeightEntries => _weightEntries.AsReadOnly();

    private Patient() { } // EF Core constructor

    public static Result<Patient> Create(Guid clinicId, string name, Species species, string breed, DateOnly birthDate, Sex? sex = null, string? microchipNumber = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Patient name is required"));

        if (string.IsNullOrWhiteSpace(breed))
            errors.Add(new ValidationError(nameof(breed), "Breed is required"));

        if (birthDate == default)
            errors.Add(new ValidationError(nameof(birthDate), "Birth date is required"));

        if (microchipNumber is not null && !MicrochipRegex.IsMatch(microchipNumber))
            errors.Add(new ValidationError(nameof(microchipNumber), "Microchip number must be 15 digits (ISO 11784/11785)"));

        if (errors.Count > 0)
            return Result<Patient>.Invalid(errors);

        var patient = new Patient
        {
            ClinicId = clinicId,
            Name = name.Trim(),
            Species = species,
            Breed = breed.Trim(),
            BirthDate = birthDate,
            Sex = sex ?? Sex.Unknown,
            MicrochipNumber = microchipNumber
        };

        return Result<Patient>.Success(patient);
    }

    public Result UpdateInfo(string? name, Species? species, string? breed, DateOnly? birthDate, Sex? sex = null, string? microchipNumber = null)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Error("Patient name cannot be empty");
            Name = name.Trim();
        }

        if (species is not null)
            Species = species.Value;

        if (breed is not null)
        {
            if (string.IsNullOrWhiteSpace(breed))
                return Result.Error("Breed cannot be empty");
            Breed = breed.Trim();
        }

        if (birthDate is not null)
        {
            if (birthDate.Value == default)
                return Result.Error("Birth date is invalid");
            BirthDate = birthDate.Value;
        }

        if (sex is not null)
            Sex = sex.Value;

        if (microchipNumber is not null)
        {
            if (!MicrochipRegex.IsMatch(microchipNumber))
                return Result.Error("Microchip number must be 15 digits (ISO 11784/11785)");
            MicrochipNumber = microchipNumber;
        }

        return Result.Success();
    }

    public Result SetWeight(decimal weightKg)
    {
        if (weightKg <= 0)
            return Result.Invalid(new ValidationError(nameof(weightKg), "Weight must be greater than zero"));

        WeightKg = weightKg;
        return Result.Success();
    }

    public Result AddOwner(PatientOwner patientOwner)
    {
        if (patientOwner is null)
            return Result.Error("PatientOwner cannot be null");

        if (_patientOwners.Any(po => po.OwnerId == patientOwner.OwnerId))
            return Result.Error("This owner is already linked to the patient");

        _patientOwners.Add(patientOwner);
        return Result.Success();
    }

    public PatientDto ToDto()
    {
        var firstOwner = PatientOwners.FirstOrDefault(po => po.Owner is not null)?.Owner;
        return new PatientDto(
            Id,
            Name,
            Species,
            Breed,
            BirthDate,
            Sex,
            firstOwner is not null ? $"{firstOwner.FirstName} {firstOwner.LastName}".Trim() : string.Empty,
            firstOwner?.Phone ?? string.Empty,
            ClinicId,
            MicrochipNumber);
    }

    [GeneratedRegex(@"^\d{15}$")]
    private static partial Regex MicrochipPattern();
}
