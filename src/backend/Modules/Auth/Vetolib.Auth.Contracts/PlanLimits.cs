namespace Vetolib.Auth.Contracts;

/// <summary>
/// Defines the resource limits for a subscription plan.
/// Use int.MaxValue for unlimited numeric limits.
/// </summary>
public record PlanLimits(
    int MaxVets,
    int MaxPatients,
    int MaxWhatsAppMessagesPerMonth,
    int MaxStorageGB,
    bool HasAiTriage,
    bool HasWhatsApp,
    bool HasMultiClinic,
    bool HasApi);
