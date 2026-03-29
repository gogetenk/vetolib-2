using Ardalis.Result;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Domain;

internal class HeatCycle : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Notes { get; private set; }

    private HeatCycle() { } // EF Core constructor

    public static Result<HeatCycle> Create(
        Guid clinicId,
        Guid patientId,
        DateOnly startDate,
        DateOnly? endDate = null,
        string? notes = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (endDate.HasValue && endDate.Value <= startDate)
            errors.Add(new ValidationError(nameof(endDate), "End date must be after start date"));

        if (errors.Count > 0)
            return Result<HeatCycle>.Invalid(errors);

        return Result<HeatCycle>.Success(new HeatCycle
        {
            ClinicId = clinicId,
            PatientId = patientId,
            StartDate = startDate,
            EndDate = endDate,
            Notes = notes?.Trim()
        });
    }

    public HeatCycleDto ToDto()
    {
        int? durationDays = EndDate.HasValue
            ? EndDate.Value.DayNumber - StartDate.DayNumber
            : null;

        return new HeatCycleDto(Id, PatientId, StartDate, EndDate, durationDays, Notes);
    }
}
