using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

internal interface ITriageOrchestrator
{
    /// <summary>
    /// Calls the AI triage service, applies emergency bias, updates the conversation,
    /// and re-routes it. Falls back gracefully if AI is unavailable.
    /// </summary>
    Task ApplyTriageAsync(
        Conversation conversation,
        string messageBody,
        CancellationToken cancellationToken);
}
