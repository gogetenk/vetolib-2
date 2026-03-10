using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

internal interface IMessageRouter
{
    /// <summary>
    /// Returns the role to assign a conversation based on its category.
    /// </summary>
    string GetAssignedRole(MessageCategory category);
}
