namespace Vetolib.Auth.Contracts;

/// <summary>
/// Static registry of plan limits per subscription tier.
/// </summary>
public static class PlanDefinitions
{
    private static readonly Dictionary<SubscriptionPlan, PlanLimits> Plans = new()
    {
        [SubscriptionPlan.Free] = new PlanLimits(
            MaxVets: 1,
            MaxPatients: 50,
            MaxWhatsAppMessagesPerMonth: 0,
            MaxStorageGB: 1,
            HasAiTriage: false,
            HasWhatsApp: false,
            HasMultiClinic: false,
            HasApi: false),

        [SubscriptionPlan.Starter] = new PlanLimits(
            MaxVets: 3,
            MaxPatients: 500,
            MaxWhatsAppMessagesPerMonth: 50,
            MaxStorageGB: 10,
            HasAiTriage: true,
            HasWhatsApp: true,
            HasMultiClinic: false,
            HasApi: false),

        [SubscriptionPlan.Pro] = new PlanLimits(
            MaxVets: int.MaxValue,
            MaxPatients: int.MaxValue,
            MaxWhatsAppMessagesPerMonth: int.MaxValue,
            MaxStorageGB: 50,
            HasAiTriage: true,
            HasWhatsApp: true,
            HasMultiClinic: false,
            HasApi: false),

        [SubscriptionPlan.Enterprise] = new PlanLimits(
            MaxVets: int.MaxValue,
            MaxPatients: int.MaxValue,
            MaxWhatsAppMessagesPerMonth: int.MaxValue,
            MaxStorageGB: int.MaxValue,
            HasAiTriage: true,
            HasWhatsApp: true,
            HasMultiClinic: true,
            HasApi: true)
    };

    /// <summary>
    /// Returns the limits for the given subscription plan.
    /// </summary>
    public static PlanLimits GetLimits(SubscriptionPlan plan) => Plans[plan];
}
