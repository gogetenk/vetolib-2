namespace Vetolib.Preferences.Contracts;

public enum PreferenceKey
{
    // Notifications
    NotificationEmail,
    NotificationPush,
    NotificationSms,
    NotificationAppointmentReminder,
    NotificationInvoice,

    // Analytics
    AnalyticsPosthog,
    AnalyticsUsageData,

    // AIFeatures
    AITriage,
    AINoShow,
    AIDrugInteractions,   // NOT configurable — ALWAYS ON (PO decision)
    AIMessaging,

    // Communication
    CommunicationQuietHoursStart,
    CommunicationQuietHoursEnd,
    CommunicationPreferredChannel,
    CommunicationLanguage,

    // Privacy
    PrivacyDataSharing,
    PrivacyMarketing,

    // Booking
    BookingEnabled,           // bool
    BookingMaxAdvanceDays,    // int
    BookingMinCancelHours,    // int
    BookingMaxReschedules,    // int
    BookingSlotGridMinutes,   // int
}
