using Ardalis.Result;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Domain;

internal class ClinicWorkingHours : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsOpen { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public TimeOnly? BreakStartTime { get; private set; }
    public TimeOnly? BreakEndTime { get; private set; }

    private ClinicWorkingHours() { } // EF Core

    public static Result<ClinicWorkingHours> Create(
        Guid clinicId,
        DayOfWeek dayOfWeek,
        bool isOpen,
        TimeOnly openTime,
        TimeOnly closeTime,
        TimeOnly? breakStartTime,
        TimeOnly? breakEndTime)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if ((int)dayOfWeek < 0 || (int)dayOfWeek > 6)
            errors.Add(new ValidationError(nameof(dayOfWeek), "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)"));

        if (isOpen)
        {
            if (closeTime <= openTime)
                errors.Add(new ValidationError(nameof(closeTime), "CloseTime must be after OpenTime"));

            if (breakStartTime.HasValue != breakEndTime.HasValue)
                errors.Add(new ValidationError(nameof(breakStartTime), "Both BreakStartTime and BreakEndTime must be provided or both omitted"));

            if (breakStartTime.HasValue && breakEndTime.HasValue)
            {
                if (breakStartTime.Value <= openTime)
                    errors.Add(new ValidationError(nameof(breakStartTime), "BreakStartTime must be after OpenTime"));

                if (breakEndTime.Value >= closeTime)
                    errors.Add(new ValidationError(nameof(breakEndTime), "BreakEndTime must be before CloseTime"));

                if (breakEndTime.Value <= breakStartTime.Value)
                    errors.Add(new ValidationError(nameof(breakEndTime), "BreakEndTime must be after BreakStartTime"));
            }
        }

        if (errors.Count > 0)
            return Result<ClinicWorkingHours>.Invalid(errors);

        return Result<ClinicWorkingHours>.Success(new ClinicWorkingHours
        {
            ClinicId = clinicId,
            DayOfWeek = dayOfWeek,
            IsOpen = isOpen,
            OpenTime = isOpen ? openTime : default,
            CloseTime = isOpen ? closeTime : default,
            BreakStartTime = isOpen ? breakStartTime : null,
            BreakEndTime = isOpen ? breakEndTime : null
        });
    }

    public Result Update(
        bool isOpen,
        TimeOnly openTime,
        TimeOnly closeTime,
        TimeOnly? breakStartTime,
        TimeOnly? breakEndTime)
    {
        var errors = new List<ValidationError>();

        if (isOpen)
        {
            if (closeTime <= openTime)
                errors.Add(new ValidationError(nameof(closeTime), "CloseTime must be after OpenTime"));

            if (breakStartTime.HasValue != breakEndTime.HasValue)
                errors.Add(new ValidationError(nameof(breakStartTime), "Both BreakStartTime and BreakEndTime must be provided or both omitted"));

            if (breakStartTime.HasValue && breakEndTime.HasValue)
            {
                if (breakStartTime.Value <= openTime)
                    errors.Add(new ValidationError(nameof(breakStartTime), "BreakStartTime must be after OpenTime"));

                if (breakEndTime.Value >= closeTime)
                    errors.Add(new ValidationError(nameof(breakEndTime), "BreakEndTime must be before CloseTime"));

                if (breakEndTime.Value <= breakStartTime.Value)
                    errors.Add(new ValidationError(nameof(breakEndTime), "BreakEndTime must be after BreakStartTime"));
            }
        }

        if (errors.Count > 0)
            return Result.Invalid(errors);

        IsOpen = isOpen;
        OpenTime = isOpen ? openTime : default;
        CloseTime = isOpen ? closeTime : default;
        BreakStartTime = isOpen ? breakStartTime : null;
        BreakEndTime = isOpen ? breakEndTime : null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public WorkingHoursDto ToDto() => new(
        Id,
        DayOfWeek,
        IsOpen,
        OpenTime,
        CloseTime,
        BreakStartTime,
        BreakEndTime);

    /// <summary>
    /// Creates UAE default working hours for a clinic (Sun-Thu 8-18, Fri 8-12, Sat closed).
    /// </summary>
    internal static List<Result<ClinicWorkingHours>> CreateUaeDefaults(Guid clinicId)
    {
        return
        [
            Create(clinicId, System.DayOfWeek.Sunday, true, new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
            Create(clinicId, System.DayOfWeek.Monday, true, new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
            Create(clinicId, System.DayOfWeek.Tuesday, true, new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
            Create(clinicId, System.DayOfWeek.Wednesday, true, new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
            Create(clinicId, System.DayOfWeek.Thursday, true, new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
            Create(clinicId, System.DayOfWeek.Friday, true, new TimeOnly(8, 0), new TimeOnly(12, 0), null, null),
            Create(clinicId, System.DayOfWeek.Saturday, false, default, default, null, null),
        ];
    }
}
