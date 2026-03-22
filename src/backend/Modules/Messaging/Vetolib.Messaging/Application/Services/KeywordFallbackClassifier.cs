using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Keyword-based message classifier used as fallback when the AI circuit breaker is open.
/// Supports English and Arabic keywords for veterinary clinic messages.
/// </summary>
internal sealed class KeywordFallbackClassifier : IMessageClassifier
{
    private static readonly string[] UrgentKeywordsEn =
    [
        "emergency", "dying", "blood", "breathing", "poison", "seizure", "collapse",
        "choking", "unconscious", "bleeding", "convulsion", "trauma"
    ];

    private static readonly string[] UrgentKeywordsAr =
    [
        "\u0637\u0648\u0627\u0631\u0626", // طوارئ (emergency)
        "\u0646\u0632\u064a\u0641",       // نزيف (bleeding)
        "\u062a\u0633\u0645\u0645",       // تسمم (poisoning)
        "\u0625\u063a\u0645\u0627\u0621"  // إغماء (fainting)
    ];

    private static readonly string[] MedicalKeywordsEn =
    [
        "vaccination", "checkup", "surgery", "diagnosis", "sick", "vomiting",
        "diarrhea", "limping", "swelling", "infection", "medication", "prescription"
    ];

    private static readonly string[] AdminKeywordsEn =
    [
        "invoice", "billing", "payment", "receipt", "account", "insurance", "refund"
    ];

    private static readonly string[] AppointmentKeywordsEn =
    [
        "appointment", "schedule", "reschedule", "cancel", "booking", "book", "slot", "availability"
    ];

    private static readonly string[] FeedbackKeywordsEn =
    [
        "feedback", "review", "complaint", "thank", "excellent", "recommend"
    ];

    private readonly ILogger<KeywordFallbackClassifier> _logger;

    public KeywordFallbackClassifier(ILogger<KeywordFallbackClassifier> logger)
    {
        _logger = logger;
    }

    public Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            var emptyResult = new MessageClassificationResult(
                ClassifiedUrgency.Normal,
                ClassifiedCategory.Unspecified,
                Confidence: 0.1);
            return Task.FromResult<MessageClassificationResult?>(emptyResult);
        }

        var textToAnalyze = conversationSubject is not null
            ? $"{conversationSubject} {messageText}"
            : messageText;

        var lowerText = textToAnalyze.ToLowerInvariant();

        // Check urgent keywords first (highest priority)
        if (ContainsAny(lowerText, UrgentKeywordsEn) || ContainsAnyExact(textToAnalyze, UrgentKeywordsAr))
        {
            _logger.LogDebug("Keyword classifier detected urgent message.");
            var urgentResult = new MessageClassificationResult(
                ClassifiedUrgency.Critical,
                ClassifiedCategory.MedicalConcern,
                Confidence: 0.7);
            return Task.FromResult<MessageClassificationResult?>(urgentResult);
        }

        // Medical keywords
        if (ContainsAny(lowerText, MedicalKeywordsEn))
        {
            var medicalResult = new MessageClassificationResult(
                ClassifiedUrgency.Normal,
                ClassifiedCategory.MedicalConcern,
                Confidence: 0.5);
            return Task.FromResult<MessageClassificationResult?>(medicalResult);
        }

        // Appointment keywords
        if (ContainsAny(lowerText, AppointmentKeywordsEn))
        {
            var appointmentResult = new MessageClassificationResult(
                ClassifiedUrgency.Normal,
                ClassifiedCategory.AppointmentRequest,
                Confidence: 0.5);
            return Task.FromResult<MessageClassificationResult?>(appointmentResult);
        }

        // Admin keywords
        if (ContainsAny(lowerText, AdminKeywordsEn))
        {
            var adminResult = new MessageClassificationResult(
                ClassifiedUrgency.Low,
                ClassifiedCategory.AdministrativeRequest,
                Confidence: 0.5);
            return Task.FromResult<MessageClassificationResult?>(adminResult);
        }

        // Feedback keywords
        if (ContainsAny(lowerText, FeedbackKeywordsEn))
        {
            var feedbackResult = new MessageClassificationResult(
                ClassifiedUrgency.Low,
                ClassifiedCategory.Feedback,
                Confidence: 0.4);
            return Task.FromResult<MessageClassificationResult?>(feedbackResult);
        }

        // Default: no keywords matched
        var defaultResult = new MessageClassificationResult(
            ClassifiedUrgency.Normal,
            ClassifiedCategory.Unspecified,
            Confidence: 0.3);
        return Task.FromResult<MessageClassificationResult?>(defaultResult);
    }

    private static bool ContainsAny(string lowerText, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (lowerText.Contains(keyword, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static bool ContainsAnyExact(string text, string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
