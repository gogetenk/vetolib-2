using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class ConsultationType : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }
    public bool RequiresVetSelection { get; private set; }

    private ConsultationType() { } // EF Core constructor

    public static Result<ConsultationType> Create(
        Guid clinicId,
        string name,
        int durationMinutes,
        int sortOrder = 0,
        bool requiresVetSelection = false)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));
        else if (name.Length > 100)
            errors.Add(new ValidationError(nameof(name), "Name must not exceed 100 characters"));

        if (durationMinutes < 10 || durationMinutes > 180)
            errors.Add(new ValidationError(nameof(durationMinutes), "Duration must be between 10 and 180 minutes"));

        if (errors.Count > 0)
            return Result<ConsultationType>.Invalid(errors);

        return Result<ConsultationType>.Success(new ConsultationType
        {
            ClinicId = clinicId,
            Name = name.Trim(),
            DurationMinutes = durationMinutes,
            IsActive = true,
            SortOrder = sortOrder,
            RequiresVetSelection = requiresVetSelection
        });
    }

    public Result Update(string name, int durationMinutes, int sortOrder, bool requiresVetSelection)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));
        else if (name.Length > 100)
            errors.Add(new ValidationError(nameof(name), "Name must not exceed 100 characters"));

        if (durationMinutes < 10 || durationMinutes > 180)
            errors.Add(new ValidationError(nameof(durationMinutes), "Duration must be between 10 and 180 minutes"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name.Trim();
        DurationMinutes = durationMinutes;
        SortOrder = sortOrder;
        RequiresVetSelection = requiresVetSelection;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Error("ConsultationType is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public ConsultationTypeDto ToDto() =>
        new(Id, Name, DurationMinutes, IsActive, SortOrder, RequiresVetSelection);
}
