using Vetolib.Notifications.Contracts.Enums;

namespace Vetolib.Notifications.Contracts.Dtos;

public record ReminderLogDto(
    Guid Id,
    Guid? AppointmentId,
    Guid? PatientId,
    ReminderType ReminderType,
    DateTime SentAt,
    NotificationChannel Channel,
    DeliveryStatus DeliveryStatus,
    string RecipientEmail);
