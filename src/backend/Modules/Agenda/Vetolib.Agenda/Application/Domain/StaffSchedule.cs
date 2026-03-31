using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class StaffSchedule : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public bool IsAvailable { get; private set; }

    private StaffSchedule() { } // EF Core constructor

    public static Result<StaffSchedule> Create(
        Guid clinicId,
        Guid userId,
        string userName,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        ShiftType shiftType,
        bool isAvailable = true)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (userId == Guid.Empty)
            errors.Add(new ValidationError(nameof(userId), "UserId is required"));

        if (string.IsNullOrWhiteSpace(userName))
            errors.Add(new ValidationError(nameof(userName), "User name is required"));

        if (endTime <= startTime)
            errors.Add(new ValidationError(nameof(endTime), "End time must be after start time"));

        if (errors.Count > 0)
            return Result<StaffSchedule>.Invalid(errors);

        var schedule = new StaffSchedule
        {
            ClinicId = clinicId,
            UserId = userId,
            UserName = userName,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            ShiftType = shiftType,
            IsAvailable = isAvailable
        };

        return Result<StaffSchedule>.Success(schedule);
    }

    public StaffScheduleDto ToDto()
    {
        return new StaffScheduleDto(
            Id,
            ClinicId,
            UserId,
            UserName,
            Date,
            StartTime,
            EndTime,
            ShiftType,
            IsAvailable);
    }
}
