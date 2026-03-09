using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class Patient : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Species Species { get; private set; }
    public string Breed { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }

    private readonly List<PatientOwner> _patientOwners = [];
    public IReadOnlyList<PatientOwner> PatientOwners => _patientOwners.AsReadOnly();

    private readonly List<MedicalRecord> _medicalRecords = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    private Patient() { } // EF Core constructor

    public static Result<Patient> Create(Guid clinicId, string name, Species species, string breed, DateOnly birthDate)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Le nom de l'animal est requis"));

        if (string.IsNullOrWhiteSpace(breed))
            errors.Add(new ValidationError(nameof(breed), "La race est requise"));

        if (birthDate == default)
            errors.Add(new ValidationError(nameof(birthDate), "La date de naissance est requise"));

        if (errors.Count > 0)
            return Result<Patient>.Invalid(errors);

        var patient = new Patient
        {
            ClinicId = clinicId,
            Name = name.Trim(),
            Species = species,
            Breed = breed.Trim(),
            BirthDate = birthDate
        };

        return Result<Patient>.Success(patient);
    }

    public Result UpdateInfo(string? name, Species? species, string? breed, DateOnly? birthDate)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Error("Le nom de l'animal ne peut pas etre vide");
            Name = name.Trim();
        }

        if (species is not null)
            Species = species.Value;

        if (breed is not null)
        {
            if (string.IsNullOrWhiteSpace(breed))
                return Result.Error("La race ne peut pas etre vide");
            Breed = breed.Trim();
        }

        if (birthDate is not null)
        {
            if (birthDate.Value == default)
                return Result.Error("La date de naissance est invalide");
            BirthDate = birthDate.Value;
        }

        return Result.Success();
    }

    public void AddOwner(PatientOwner patientOwner)
    {
        _patientOwners.Add(patientOwner);
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
            firstOwner is not null ? $"{firstOwner.FirstName} {firstOwner.LastName}".Trim() : string.Empty,
            firstOwner?.Phone ?? string.Empty,
            ClinicId);
    }
}
