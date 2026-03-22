using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.AI.Contracts;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Orchestrates AI triage for incoming owner messages.
/// - Calls IMessageTriageService (optional — falls back if null/unavailable)
/// - Applies emergency bias: MedicalQuestion vs MedicalUrgency uncertainty → MedicalUrgency
/// - Marks IsTriageUncertain when confidence &lt; configured threshold
/// - Re-routes conversation via IMessageRouter after triage
/// </summary>
internal sealed class TriageOrchestrator : ITriageOrchestrator
{
    private readonly double _uncertaintyThreshold;

    private static readonly HashSet<string> MedicalUrgencyAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "MedicalUrgency",
            "Medical Urgency",
            "Urgency",
            "Emergency"
        };

    private static readonly HashSet<string> MedicalQuestionAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "MedicalQuestion",
            "Medical Question",
            "MedicalAdvice"
        };

    private readonly IMessageTriageService? _triageService;
    private readonly IMessageRouter _router;
    private readonly ILogger<TriageOrchestrator> _logger;

    public TriageOrchestrator(
        IMessageRouter router,
        ILogger<TriageOrchestrator> logger,
        IOptions<MessagingOptions> options,
        IMessageTriageService? triageService = null)
    {
        _router = router;
        _logger = logger;
        _triageService = triageService;
        _uncertaintyThreshold = options.Value.TriageUncertaintyThreshold;
    }

    public async Task ApplyTriageAsync(
        Conversation conversation,
        string messageBody,
        CancellationToken cancellationToken)
    {
        if (_triageService is null)
        {
            // AI not registered — route to receptionist with uncertain flag
            conversation.SetTriageResult(0m, isUncertain: true);
            conversation.AssignTo(null, "Receptionist");
            return;
        }

        MessageTriageResult? triage = null;

        try
        {
            triage = await _triageService.TriageAsync(
                messageBody,
                additionalContext: conversation.Subject,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI triage failed for conversation {ConversationId} — falling back to receptionist", conversation.Id);
        }

        if (triage is null)
        {
            conversation.SetTriageResult(0m, isUncertain: true);
            conversation.AssignTo(null, "Receptionist");
            return;
        }

        var isUncertain = triage.Confidence < _uncertaintyThreshold;
        var category = ResolveCategory(triage.Category, triage.Confidence, isUncertain, conversation.Category);

        conversation.UpdateCategory(category);
        conversation.SetTriageResult((decimal)triage.Confidence, isUncertain);
        conversation.AssignTo(null, _router.GetAssignedRole(category));
    }

    /// <summary>
    /// Maps the AI category string to the domain enum.
    /// Emergency bias: if uncertain between MedicalQuestion and MedicalUrgency → choose MedicalUrgency.
    /// </summary>
    private static MessageCategory ResolveCategory(
        string aiCategory,
        double confidence,
        bool isUncertain,
        MessageCategory fallback)
    {
        // Emergency bias: when confidence is low and AI suggests MedicalQuestion,
        // escalate to MedicalUrgency to avoid under-triaging medical issues.
        if (isUncertain && MedicalQuestionAliases.Contains(aiCategory))
            return MessageCategory.MedicalUrgency;

        if (MedicalUrgencyAliases.Contains(aiCategory))
            return MessageCategory.MedicalUrgency;

        return aiCategory switch
        {
            var s when MedicalQuestionAliases.Contains(s)                            => MessageCategory.MedicalQuestion,
            var s when s.Equals("PostOperativeFollowUp", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("PostOperative", StringComparison.OrdinalIgnoreCase) => MessageCategory.PostOperativeFollowUp,
            var s when s.Equals("AppointmentRequest", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("Appointment", StringComparison.OrdinalIgnoreCase)  => MessageCategory.AppointmentRequest,
            var s when s.Equals("Administrative", StringComparison.OrdinalIgnoreCase) => MessageCategory.Administrative,
            var s when s.Equals("Feedback", StringComparison.OrdinalIgnoreCase)       => MessageCategory.Feedback,
            var s when s.Equals("Other", StringComparison.OrdinalIgnoreCase)          => MessageCategory.Other,
            _                                                                          => fallback
        };
    }
}
