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

    public void MarkSent()
    {
        DeliveryStatus = DeliveryStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        DeliveryStatus = DeliveryStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
