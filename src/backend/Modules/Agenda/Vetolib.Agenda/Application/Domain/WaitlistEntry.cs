using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class WaitlistEntry : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public string OwnerPhone { get; private set; } = string.Empty;
    public string? OwnerEmail { get; private set; }
    public DateOnly PreferredDate { get; private set; }
    public PreferredTimeSlot PreferredTimeSlot { get; private set; }
    public Guid? VetPreference { get; private set; }
    public string? Reason { get; private set; }
    public WaitlistEntryStatus Status { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private WaitlistEntry() { } // EF Core constructor

    public static Result<WaitlistEntry> Create(
        Guid clinicId,
        Guid patientId,
        string ownerName,
        string ownerPhone,
        string? ownerEmail,
        DateOnly preferredDate,
        PreferredTimeSlot preferredTimeSlot,
        Guid? vetPreference,
        string? reason)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (string.IsNullOrWhiteSpace(ownerName))
            errors.Add(new ValidationError(nameof(ownerName), "Owner name is required"));

        if (string.IsNullOrWhiteSpace(ownerPhone))
            errors.Add(new ValidationError(nameof(ownerPhone), "Owner phone is required"));

        if (errors.Count > 0)
            return Result<WaitlistEntry>.Invalid(errors);

        var entry = new WaitlistEntry
        {
            ClinicId = clinicId,
            PatientId = patientId,
            OwnerName = ownerName,
            OwnerPhone = ownerPhone,
            OwnerEmail = ownerEmail,
            PreferredDate = preferredDate,
            PreferredTimeSlot = preferredTimeSlot,
            VetPreference = vetPreference,
            Reason = reason,
            Status = WaitlistEntryStatus.Pending
        };

        return Result<WaitlistEntry>.Success(entry);
    }

    public Result Notify()
    {
        if (Status != WaitlistEntryStatus.Pending)
            return Result.Error($"INVALID_TRANSITION:Cannot notify a waitlist entry with status {Status}");

        Status = WaitlistEntryStatus.Notified;
        NotifiedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(48);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkBooked()
    {
        if (Status != WaitlistEntryStatus.Notified)
            return Result.Error($"INVALID_TRANSITION:Cannot book a waitlist entry with status {Status}");

        Status = WaitlistEntryStatus.Booked;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status != WaitlistEntryStatus.Notified)
            return Result.Error($"INVALID_TRANSITION:Cannot expire a waitlist entry with status {Status}");

        Status = WaitlistEntryStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public WaitlistEntryDto ToDto()
    {
        return new WaitlistEntryDto(
            Id,
            ClinicId,
            PatientId,
            OwnerName,
            OwnerPhone,
            OwnerEmail,
            PreferredDate,
            PreferredTimeSlot,
            VetPreference,
            Reason,
            Status,
            CreatedAt,
            NotifiedAt,
            ExpiresAt);
    }
}
