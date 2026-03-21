namespace Vetolib.Auth.Contracts;

/// <summary>
/// Current usage metrics for a clinic, compared against plan limits.
/// </summary>
public record UsageDto(
    SubscriptionPlan Plan,
    int CurrentVets,
    int MaxVets,
    int CurrentPatients,
    int MaxPatients,
    int CurrentWhatsAppMessages,
    int MaxWhatsAppMessages,
    int CurrentStorageGB,
    int MaxStorageGB,
    bool HasAiTriage,
    bool HasWhatsApp,
    bool HasMultiClinic,
    bool HasApi);
