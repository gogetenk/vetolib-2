using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Domain;

internal static class SystemDefaults
{
    private static readonly Dictionary<PreferenceKey, string> Defaults = new()
    {
        { PreferenceKey.NotificationEmail, "true" },
        { PreferenceKey.NotificationPush, "false" },
        { PreferenceKey.NotificationSms, "false" },
        { PreferenceKey.NotificationAppointmentReminder, "true" },
        { PreferenceKey.NotificationInvoice, "true" },
        { PreferenceKey.AnalyticsPosthog, "false" },
        { PreferenceKey.AnalyticsUsageData, "false" },
        { PreferenceKey.AITriage, "true" },
        { PreferenceKey.AINoShow, "true" },
        { PreferenceKey.AIDrugInteractions, "true" },
        { PreferenceKey.AIMessaging, "true" },
        { PreferenceKey.CommunicationQuietHoursStart, "22:00" },
        { PreferenceKey.CommunicationQuietHoursEnd, "07:00" },
        { PreferenceKey.CommunicationPreferredChannel, "Email" },
        { PreferenceKey.CommunicationLanguage, "en" },
        { PreferenceKey.PrivacyDataSharing, "false" },
        { PreferenceKey.PrivacyMarketing, "false" },
    };

    private static readonly Dictionary<PreferenceKey, PreferenceCategory> Categories = new()
    {
        { PreferenceKey.NotificationEmail, PreferenceCategory.Notifications },
        { PreferenceKey.NotificationPush, PreferenceCategory.Notifications },
        { PreferenceKey.NotificationSms, PreferenceCategory.Notifications },
        { PreferenceKey.NotificationAppointmentReminder, PreferenceCategory.Notifications },
        { PreferenceKey.NotificationInvoice, PreferenceCategory.Notifications },
        { PreferenceKey.AnalyticsPosthog, PreferenceCategory.Analytics },
        { PreferenceKey.AnalyticsUsageData, PreferenceCategory.Analytics },
        { PreferenceKey.AITriage, PreferenceCategory.AIFeatures },
        { PreferenceKey.AINoShow, PreferenceCategory.AIFeatures },
        { PreferenceKey.AIDrugInteractions, PreferenceCategory.AIFeatures },
        { PreferenceKey.AIMessaging, PreferenceCategory.AIFeatures },
        { PreferenceKey.CommunicationQuietHoursStart, PreferenceCategory.Communication },
        { PreferenceKey.CommunicationQuietHoursEnd, PreferenceCategory.Communication },
        { PreferenceKey.CommunicationPreferredChannel, PreferenceCategory.Communication },
        { PreferenceKey.CommunicationLanguage, PreferenceCategory.Communication },
        { PreferenceKey.PrivacyDataSharing, PreferenceCategory.Privacy },
        { PreferenceKey.PrivacyMarketing, PreferenceCategory.Privacy },
    };

    public static string GetDefault(PreferenceKey key)
    {
        if (Defaults.TryGetValue(key, out var value))
            return value;
        throw new InvalidOperationException(
            $"No system default defined for PreferenceKey '{key}'. " +
            $"Every PreferenceKey must have a hardcoded default in SystemDefaults.");
    }

    public static PreferenceCategory GetCategory(PreferenceKey key)
    {
        if (Categories.TryGetValue(key, out var category))
            return category;
        throw new InvalidOperationException(
            $"No category mapping defined for PreferenceKey '{key}'. " +
            $"Every PreferenceKey must have a category in SystemDefaults.");
    }

    public static bool HasDefault(PreferenceKey key) => Defaults.ContainsKey(key);

    public static IReadOnlyDictionary<PreferenceKey, string> All => Defaults;
}
