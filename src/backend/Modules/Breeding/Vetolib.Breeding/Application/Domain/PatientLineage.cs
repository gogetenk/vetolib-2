using Ardalis.Result;
using Vetolib.Breeding.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Domain;

internal class PatientLineage : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? MotherPatientId { get; private set; }
    public Guid? FatherPatientId { get; private set; }
    public string? RegistryNumber { get; private set; }
    public RegistryType? RegistryType { get; private set; }

    private PatientLineage() { } // EF Core constructor

    public static Result<PatientLineage> Create(
        Guid clinicId,
        Guid patientId,
        Guid? motherPatientId,
        Guid? fatherPatientId,
        string? registryNumber,
        RegistryType? registryType)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (motherPatientId == patientId && patientId != Guid.Empty)
            errors.Add(new ValidationError(nameof(motherPatientId), "A patient cannot be its own mother"));

        if (fatherPatientId == patientId && patientId != Guid.Empty)
            errors.Add(new ValidationError(nameof(fatherPatientId), "A patient cannot be its own father"));

        if (errors.Count > 0)
            return Result<PatientLineage>.Invalid(errors);

        return Result<PatientLineage>.Success(new PatientLineage
        {
            ClinicId = clinicId,
            PatientId = patientId,
            MotherPatientId = motherPatientId,
            FatherPatientId = fatherPatientId,
            RegistryNumber = registryNumber?.Trim(),
            RegistryType = registryType
        });
    }

    public Result SetParents(
        Guid? motherPatientId,
        Guid? fatherPatientId)
    {
        if (motherPatientId == PatientId)
            return Result.Error("A patient cannot be its own mother");

        if (fatherPatientId == PatientId)
            return Result.Error("A patient cannot be its own father");

        MotherPatientId = motherPatientId;
        FatherPatientId = fatherPatientId;
        return Result.Success();
    }

    public Result SetRegistry(string? registryNumber, RegistryType? registryType)
    {
        RegistryNumber = registryNumber?.Trim();
        RegistryType = registryType;
        return Result.Success();
    }

    /// <summary>
    /// Validates that the proposed parents are compatible with the offspring.
    /// Must be called with resolved patient info from IPatientReader.
    /// </summary>
    public static Result ValidateParentCompatibility(
        PatientBasicInfoDto offspring,
        PatientBasicInfoDto? mother,
        PatientBasicInfoDto? father)
    {
        var errors = new List<string>();

        if (mother is not null)
        {
            if (mother.Species != offspring.Species)
                errors.Add("Parent and offspring must be the same species");

            if (mother.Sex != Sex.Female && mother.Sex != Sex.SpayedFemale)
                errors.Add("Mother must be female");
        }

        if (father is not null)
        {
            if (father.Species != offspring.Species)
                errors.Add("Parent and offspring must be the same species");

            if (father.Sex != Sex.Male && father.Sex != Sex.NeuteredMale)
                errors.Add("Father must be male");
        }

        if (errors.Count > 0)
            return Result.Error(string.Join("; ", errors));

        return Result.Success();
    }
}
