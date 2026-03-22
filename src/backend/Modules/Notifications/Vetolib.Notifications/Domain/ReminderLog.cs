using Ardalis.Result;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Shared.Kernel;

namespace Vetolib.Notifications.Domain;

internal class ReminderLog : BaseEntity, IMultiTenant
{
    public Guid? AppointmentId { get; private set; }
    public Guid? PatientId { get; private set; }
    public ReminderType ReminderType { get; private set; }
    public DateTime SentAt { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public DeliveryStatus DeliveryStatus { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public Guid ClinicId { get; private set; }

    private ReminderLog() { }

    public static Result<ReminderLog> Create(
        ReminderType reminderType,
        NotificationChannel channel,
        string recipientEmail,
        Guid clinicId,
        Guid? appointmentId = null,
        Guid? patientId = null)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            return Result<ReminderLog>.Invalid(new ValidationError("RecipientEmail is required."));

        var log = new ReminderLog
        {
            ReminderType = reminderType,
            Channel = channel,
            RecipientEmail = recipientEmail,
            ClinicId = clinicId,
            AppointmentId = appointmentId,
            PatientId = patientId,
            SentAt = DateTime.UtcNow,
            DeliveryStatus = DeliveryStatus.Pending
        };

        return Result<ReminderLog>.Success(log);
    }

    public Result MarkSent()
    {
        if (DeliveryStatus == DeliveryStatus.Sent)
            return Result.Error("Reminder has already been marked as sent");

        DeliveryStatus = DeliveryStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkFailed()
    {
        if (DeliveryStatus == DeliveryStatus.Failed)
            return Result.Error("Reminder has already been marked as failed");

        DeliveryStatus = DeliveryStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
