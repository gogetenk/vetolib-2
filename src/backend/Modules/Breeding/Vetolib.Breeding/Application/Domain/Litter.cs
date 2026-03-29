using Ardalis.Result;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Domain;

internal class Litter : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid MotherPatientId { get; private set; }
    public Guid? FatherPatientId { get; private set; }
    public string? ExternalFatherName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public int BornCount { get; private set; }
    public int AliveCount { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<LitterOffspring> _offspring = [];
    public IReadOnlyList<LitterOffspring> Offspring => _offspring.AsReadOnly();

    private Litter() { } // EF Core constructor

    public static Result<Litter> Create(
        Guid clinicId,
        Guid motherPatientId,
        Guid? fatherPatientId,
        string? externalFatherName,
        DateOnly birthDate,
        int bornCount,
        int aliveCount,
        string? notes)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (motherPatientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(motherPatientId), "Mother patient is required"));

        if (fatherPatientId.HasValue && !string.IsNullOrWhiteSpace(externalFatherName))
            errors.Add(new ValidationError(nameof(fatherPatientId), "Cannot specify both a registered father and an external father name"));

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add(new ValidationError(nameof(birthDate), "Birth date cannot be in the future"));

        if (bornCount < 0)
            errors.Add(new ValidationError(nameof(bornCount), "Born count must be non-negative"));

        if (aliveCount < 0)
            errors.Add(new ValidationError(nameof(aliveCount), "Alive count must be non-negative"));

        if (aliveCount > bornCount)
            errors.Add(new ValidationError(nameof(aliveCount), "Alive count cannot exceed born count"));

        if (errors.Count > 0)
            return Result<Litter>.Invalid(errors);

        return Result<Litter>.Success(new Litter
        {
            ClinicId = clinicId,
            MotherPatientId = motherPatientId,
            FatherPatientId = fatherPatientId,
            ExternalFatherName = externalFatherName?.Trim(),
            BirthDate = birthDate,
            BornCount = bornCount,
            AliveCount = aliveCount,
            Notes = notes?.Trim()
        });
    }

    public Result AddOffspring(Guid patientId, int? birthOrder)
    {
        if (patientId == Guid.Empty)
            return Result.Error("Patient ID is required");

        if (_offspring.Any(o => o.PatientId == patientId))
            return Result.Error("This patient is already registered as offspring in this litter");

        _offspring.Add(LitterOffspring.Create(ClinicId, Id, patientId, birthOrder));
        return Result.Success();
    }

    public LitterDto ToDto()
    {
        return new LitterDto(
            Id,
            MotherPatientId,
            FatherPatientId,
            ExternalFatherName,
            BirthDate,
            BornCount,
            AliveCount,
            Notes,
            _offspring.Select(o => new LitterOffspringDto(o.Id, o.LitterId, o.PatientId, o.BirthOrder)).ToList());
    }
}
