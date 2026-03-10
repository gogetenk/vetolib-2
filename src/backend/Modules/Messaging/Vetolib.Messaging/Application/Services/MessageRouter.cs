using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Routes conversations to the appropriate staff role based on AI-assigned category.
/// MedicalUrgency/PostOperativeFollowUp/MedicalQuestion → Vet
/// AppointmentRequest/Administrative/Other → Receptionist
/// Feedback → Admin
/// </summary>
internal sealed class MessageRouter : IMessageRouter
{
    private static readonly Dictionary<MessageCategory, string> RoleByCategory = new()
    {
        [MessageCategory.MedicalUrgency]        = "Vet",
        [MessageCategory.PostOperativeFollowUp] = "Vet",
        [MessageCategory.MedicalQuestion]       = "Vet",
        [MessageCategory.AppointmentRequest]    = "Receptionist",
        [MessageCategory.Administrative]        = "Receptionist",
        [MessageCategory.Feedback]              = "Admin",
        [MessageCategory.Other]                 = "Receptionist"
    };

    public string GetAssignedRole(MessageCategory category) =>
        RoleByCategory.GetValueOrDefault(category, "Receptionist");
}
