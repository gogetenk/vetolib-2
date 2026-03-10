using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class MessagingHours : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    private MessagingHours() { } // EF Core

    public static Result<MessagingHours> Create(
        Guid clinicId,
        int dayOfWeek,
        TimeOnly openTime,
        TimeOnly closeTime,
        bool isClosed = false)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (dayOfWeek is < 0 or > 6)
            errors.Add(new ValidationError(nameof(dayOfWeek), "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)"));

        if (!isClosed && openTime >= closeTime)
            errors.Add(new ValidationError(nameof(openTime), "OpenTime must be before CloseTime"));

        if (errors.Count > 0)
            return Result<MessagingHours>.Invalid(errors);

        return Result<MessagingHours>.Success(new MessagingHours
        {
            ClinicId = clinicId,
            DayOfWeek = dayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime,
            IsClosed = isClosed
        });
    }

    public Result Update(TimeOnly openTime, TimeOnly closeTime, bool isClosed)
    {
        if (!isClosed && openTime >= closeTime)
            return Result.Invalid(new ValidationError(nameof(openTime), "OpenTime must be before CloseTime"));

        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;

        return Result.Success();
    }

    public bool IsOpenAt(TimeOnly time)
    {
        if (IsClosed) return false;
        return time >= OpenTime && time < CloseTime;
    }

    public MessagingHoursDto ToDto() => new(
        Id,
        ClinicId,
        DayOfWeek,
        OpenTime,
        CloseTime,
        IsClosed);
}
