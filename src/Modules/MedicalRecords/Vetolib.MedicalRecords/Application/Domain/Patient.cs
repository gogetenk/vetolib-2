using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class Patient : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Species { get; private set; } = string.Empty;
    public string Breed { get; private set; } = string.Empty;
    public DateTime? DateOfBirth { get; private set; }

    private readonly List<PatientOwner> _patientOwners = [];
    public IReadOnlyList<PatientOwner> PatientOwners => _patientOwners.AsReadOnly();

    private Patient() { } // EF Core constructor

    public static Result<Patient> Create(Guid clinicId, string name, string species, string breed, DateTime? dateOfBirth)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Le nom de l'animal est requis"));

        if (string.IsNullOrWhiteSpace(species))
            errors.Add(new ValidationError(nameof(species), "L'espece est requise"));

        if (string.IsNullOrWhiteSpace(breed))
            errors.Add(new ValidationError(nameof(breed), "La race est requise"));

        if (errors.Count > 0)
            return Result<Patient>.Invalid(errors);

        var patient = new Patient
        {
            ClinicId = clinicId,
            Name = name.Trim(),
            Species = species.Trim(),
            Breed = breed.Trim(),
            DateOfBirth = dateOfBirth
        };

        return Result<Patient>.Success(patient);
    }

    public void AddOwner(PatientOwner patientOwner)
    {
        _patientOwners.Add(patientOwner);
    }

    public PatientDto ToDto()
    {
        return new PatientDto(
            Id,
            Name,
            Species,
            Breed,
            DateOfBirth,
            ClinicId,
            CreatedAt,
            PatientOwners
                .Where(po => po.Owner is not null)
                .Select(po => po.Owner!.ToDto())
                .ToList());
    }
}
