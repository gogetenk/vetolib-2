using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class Appointment : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public string VeterinarianName { get; private set; } = string.Empty;
    public Guid AnimalId { get; private set; }
    public string AnimalName { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public string? OwnerEmail { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public bool ReminderSent { get; private set; }
    public BookingSource Source { get; private set; } = BookingSource.Staff;
    public int RescheduleCount { get; private set; }
    public Guid? OriginalAppointmentId { get; private set; }

    private Appointment() { } // EF Core constructor

    public static Result<Appointment> Create(
        Guid clinicId,
        Guid veterinarianId,
        string veterinarianName,
        Guid animalId,
        string animalName,
        string ownerName,
        string? ownerEmail,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        string? reason,
        BookingSource source = BookingSource.Staff)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (veterinarianId == Guid.Empty)
            errors.Add(new ValidationError(nameof(veterinarianId), "VeterinarianId is required"));

        if (string.IsNullOrWhiteSpace(veterinarianName))
            errors.Add(new ValidationError(nameof(veterinarianName), "Veterinarian name is required"));

        if (animalId == Guid.Empty)
            errors.Add(new ValidationError(nameof(animalId), "AnimalId is required"));

        if (string.IsNullOrWhiteSpace(animalName))
            errors.Add(new ValidationError(nameof(animalName), "Animal name is required"));

        if (string.IsNullOrWhiteSpace(ownerName))
            errors.Add(new ValidationError(nameof(ownerName), "Owner name is required"));

        if (durationMinutes <= 0)
            errors.Add(new ValidationError(nameof(durationMinutes), "Duration must be greater than 0"));

        if (errors.Count > 0)
            return Result<Appointment>.Invalid(errors);

        var endTime = startTime.AddMinutes(durationMinutes);

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            VeterinarianId = veterinarianId,
            VeterinarianName = veterinarianName,
            AnimalId = animalId,
            AnimalName = animalName,
            OwnerName = ownerName,
            OwnerEmail = ownerEmail,
            Date = date,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            EndTime = endTime,
            Status = AppointmentStatus.Scheduled,
            Reason = reason,
            Source = source
        };

        return Result<Appointment>.Success(appointment);
    }

    public Result CheckIn()
    {
        if (Status != AppointmentStatus.Scheduled)
            return Result.Error($"INVALID_TRANSITION:Cannot transition to CHECKED_IN from {Status}");
        Status = AppointmentStatus.CheckedIn;
        return Result.Success();
    }

    public Result StartConsultation()
    {
        if (Status != AppointmentStatus.CheckedIn)
            return Result.Error($"INVALID_TRANSITION:Cannot transition to IN_PROGRESS from {Status}");
        Status = AppointmentStatus.InProgress;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            return Result.Error($"INVALID_TRANSITION:Cannot transition to COMPLETED from {Status}");
        Status = AppointmentStatus.Completed;
        return Result.Success();
    }

    public Result Cancel(string? reason = null)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            return Result.Error($"INVALID_TRANSITION:Cannot cancel an appointment with status {Status}");
        Status = AppointmentStatus.Cancelled;
        if (reason is not null)
            Reason = reason;
        return Result.Success();
    }

    public Result MarkNoShow()
    {
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.CheckedIn)
            return Result.Error($"INVALID_TRANSITION:Cannot transition to NO_SHOW from {Status}");
        Status = AppointmentStatus.NoShow;
        return Result.Success();
    }

    public Result MarkReminderSent()
    {
        ReminderSent = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reschedule(
        DateOnly? newDate,
        TimeOnly? newStartTime,
        int? newDurationMinutes,
        Guid? newVeterinarianId,
        string? newVeterinarianName,
        string? newReason,
        string? newNotes)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            return Result.Error($"INVALID_TRANSITION:Cannot modify an appointment with status {Status}");

        if (newDate.HasValue)
            Date = newDate.Value;

        if (newStartTime.HasValue)
            StartTime = newStartTime.Value;

        if (newDurationMinutes.HasValue && newDurationMinutes.Value > 0)
            DurationMinutes = newDurationMinutes.Value;

        EndTime = StartTime.AddMinutes(DurationMinutes);

        if (newVeterinarianId.HasValue && newVeterinarianId.Value != Guid.Empty)
            VeterinarianId = newVeterinarianId.Value;

        if (!string.IsNullOrWhiteSpace(newVeterinarianName))
            VeterinarianName = newVeterinarianName;

        if (newReason is not null)
            Reason = newReason;

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result IncrementReschedule(Guid newAppointmentId, int maxReschedules)
    {
        if (RescheduleCount >= maxReschedules)
            return Result.Error("RESCHEDULE_LIMIT_REACHED:Maximum number of reschedules reached");
        RescheduleCount++;
        OriginalAppointmentId = newAppointmentId;
        return Result.Success();
    }

    public bool OverlapsWith(TimeOnly otherStart, TimeOnly otherEnd)
    {
        return StartTime < otherEnd && EndTime > otherStart;
    }

    public AppointmentDto ToDto()
    {
        return new AppointmentDto(
            Id,
            ClinicId,
            VeterinarianId,
            VeterinarianName,
            AnimalId,
            AnimalName,
            OwnerName,
            Date,
            StartTime,
            DurationMinutes,
            EndTime,
            Status,
            Reason,
            Source,
            RescheduleCount,
            OriginalAppointmentId);
    }
}
