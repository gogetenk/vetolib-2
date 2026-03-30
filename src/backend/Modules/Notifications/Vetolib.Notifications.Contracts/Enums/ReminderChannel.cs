namespace Vetolib.Notifications.Contracts.Enums;

/// <summary>
/// Preferred channel for sending appointment reminders.
/// Clinics can choose Email-only, WhatsApp-only, or Both.
/// </summary>
public enum ReminderChannel
{
    Email = 0,
    WhatsApp = 1,
    Both = 2
}
