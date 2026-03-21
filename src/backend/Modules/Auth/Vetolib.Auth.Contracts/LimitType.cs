namespace Vetolib.Auth.Contracts;

/// <summary>
/// Types of limits enforced by subscription plans.
/// </summary>
public enum LimitType
{
    Vets,
    Patients,
    WhatsAppMessages,
    StorageGB,
    AiTriage,
    WhatsApp,
    MultiClinic,
    Api
}
