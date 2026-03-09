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
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Reason { get; private set; }

    private Appointment() { } // EF Core constructor

    public static Result<Appointment> Create(
        Guid clinicId,
        Guid veterinarianId,
        string veterinarianName,
        Guid animalId,
        string animalName,
        string ownerName,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        string? reason)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (veterinarianId == Guid.Empty)
            errors.Add(new ValidationError(nameof(veterinarianId), "VeterinarianId is required"));

        if (string.IsNullOrWhiteSpace(veterinarianName))
            errors.Add(new ValidationError(nameof(veterinarianName), "Le nom du veterinaire est requis"));

        if (animalId == Guid.Empty)
            errors.Add(new ValidationError(nameof(animalId), "AnimalId is required"));

        if (string.IsNullOrWhiteSpace(animalName))
            errors.Add(new ValidationError(nameof(animalName), "Le nom de l'animal est requis"));

        if (string.IsNullOrWhiteSpace(ownerName))
            errors.Add(new ValidationError(nameof(ownerName), "Le nom du proprietaire est requis"));

        if (durationMinutes <= 0)
            errors.Add(new ValidationError(nameof(durationMinutes), "La duree doit etre superieure a 0"));

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
            Date = date,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            EndTime = endTime,
            Status = AppointmentStatus.Scheduled,
            Reason = reason
        };

        return Result<Appointment>.Success(appointment);
    }

    public Result CheckIn()
    {
        if (Status != AppointmentStatus.Scheduled)
            return Result.Error($"INVALID_TRANSITION:Impossible de passer en CHECKED_IN depuis {Status}");
        Status = AppointmentStatus.CheckedIn;
        return Result.Success();
    }

    public Result StartConsultation()
    {
        if (Status != AppointmentStatus.CheckedIn)
            return Result.Error($"INVALID_TRANSITION:Impossible de passer en IN_PROGRESS depuis {Status}");
        Status = AppointmentStatus.InProgress;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            return Result.Error($"INVALID_TRANSITION:Impossible de passer en COMPLETED depuis {Status}");
        Status = AppointmentStatus.Completed;
        return Result.Success();
    }

    public Result Cancel(string? reason = null)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            return Result.Error($"INVALID_TRANSITION:Impossible d'annuler un rendez-vous {Status}");
        Status = AppointmentStatus.Cancelled;
        if (reason is not null)
            Reason = reason;
        return Result.Success();
    }

    public Result MarkNoShow()
    {
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.CheckedIn)
            return Result.Error($"INVALID_TRANSITION:Impossible de passer en NO_SHOW depuis {Status}");
        Status = AppointmentStatus.NoShow;
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
            Reason);
    }
}
