using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Domain;

/// <summary>
/// Hardcoded system defaults for all PreferenceKey values.
/// Resolution cascade: User > Clinic > System (this class).
/// </summary>
internal static class SystemDefaults
{
    private static readonly Dictionary<PreferenceKey, string> Defaults = new()
    {
        // Notifications — email ON, push/sms OFF, reminders ON, invoices ON
        { PreferenceKey.NotificationEmail, "true" },
        { PreferenceKey.NotificationPush, "false" },
        { PreferenceKey.NotificationSms, "false" },
        { PreferenceKey.NotificationAppointmentReminder, "true" },
        { PreferenceKey.NotificationInvoice, "true" },

        // Analytics — PostHog OFF, usage data OFF (GDPR-ready)
        { PreferenceKey.AnalyticsPosthog, "false" },
        { PreferenceKey.AnalyticsUsageData, "false" },

        // AI Features — all ON (AIDrugInteractions non-modifiable by PO decision)
        { PreferenceKey.AITriage, "true" },
        { PreferenceKey.AINoShow, "true" },
        { PreferenceKey.AIDrugInteractions, "true" },
        { PreferenceKey.AIMessaging, "true" },

        // Communication
        { PreferenceKey.CommunicationQuietHoursStart, "22:00" },
        { PreferenceKey.CommunicationQuietHoursEnd, "07:00" },
        { PreferenceKey.CommunicationPreferredChannel, "Email" },
        { PreferenceKey.CommunicationLanguage, "en" },

        // Privacy — data sharing OFF, marketing OFF
        { PreferenceKey.PrivacyDataSharing, "false" },
        { PreferenceKey.PrivacyMarketing, "false" },
    };

    public static string GetDefault(PreferenceKey key)
    {
        if (Defaults.TryGetValue(key, out var value))
            return value;

        // This should never happen if Defaults covers all PreferenceKey values
        throw new InvalidOperationException(
            $"No system default defined for PreferenceKey '{key}'. " +
            $"Every PreferenceKey must have a hardcoded default in SystemDefaults.");
    }

    public static bool HasDefault(PreferenceKey key) => Defaults.ContainsKey(key);

    public static IReadOnlyDictionary<PreferenceKey, string> All => Defaults;
}
