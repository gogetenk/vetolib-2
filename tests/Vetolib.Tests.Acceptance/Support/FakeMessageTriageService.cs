using Vetolib.AI.Contracts;

namespace Vetolib.Tests.Acceptance.Support;

/// <summary>
/// Test double for IMessageTriageService.
/// Returns deterministic triage results based on message content so that
/// acceptance tests do not depend on a real LLM service.
/// Routing rules:
///   Emergency keywords (chocolate, trembling, not breathing, bleeding, collapsed) → MedicalUrgency (0.95)
///   Post-op keywords (stitches, surgery, swollen, post-operative) → PostOperativeFollowUp (0.90)
///   Medical keywords (sneezing, vomiting, eating, not moved) → MedicalQuestion (0.85)
///   Appointment keywords (book, appointment) → AppointmentRequest (0.88)
///   Admin keywords (opening hours, hours, schedule) → Administrative (0.88)
///   Feedback keywords (thank you, excellent, great) → Feedback (0.90)
///   Ambiguous → Other (0.50)
/// </summary>
internal sealed class FakeMessageTriageService : IMessageTriageService
{
    private static readonly string[] EmergencyKeywords =
        ["chocolate", "trembling", "not breathing", "bleeding", "collapsed", "not responding", "cannot stand"];

    private static readonly string[] PostOpKeywords =
        ["stitches", "surgery", "swollen", "post-operative", "suture", "wound"];

    private static readonly string[] MedicalKeywords =
        ["sneezing", "vomiting", "eating", "not moved", "symptoms", "sick", "diarrhea", "limping"];

    private static readonly string[] AppointmentKeywords =
        ["book", "appointment", "schedule a visit"];

    private static readonly string[] AdminKeywords =
        ["opening hours", "hours on", "schedule", "parking", "payment methods"];

    private static readonly string[] FeedbackKeywords =
        ["thank you", "excellent", "great care", "wonderful", "appreciate"];

    public Task<MessageTriageResult?> TriageAsync(
        string messageContent,
        string? additionalContext,
        CancellationToken cancellationToken = default)
    {
        var content = messageContent.ToLowerInvariant();

        if (ContainsAny(content, EmergencyKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "MedicalUrgency",
                0.95,
                ["We understand your concern. Please bring your pet to the clinic immediately for emergency care."],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        if (ContainsAny(content, PostOpKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "PostOperativeFollowUp",
                0.90,
                ["Thank you for reporting this. Post-operative monitoring is important. We will review your message promptly."],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        if (ContainsAny(content, FeedbackKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "Feedback",
                0.90,
                ["Thank you for your kind feedback! We are glad we could help."],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        if (ContainsAny(content, MedicalKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "MedicalQuestion",
                0.85,
                [
                    "Thank you for reaching out. A veterinarian will review your message shortly.",
                    "In the meantime, please monitor your pet and note any changes.",
                    "If symptoms worsen, please contact us immediately or visit the clinic."
                ],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        if (ContainsAny(content, AppointmentKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "AppointmentRequest",
                0.88,
                ["We would be happy to help you schedule an appointment. Our next available slots are listed below."],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        if (ContainsAny(content, AdminKeywords))
        {
            return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
                "Administrative",
                0.88,
                ["Our clinic is open Sunday through Thursday, 08:00 to 20:00."],
                "This is an AI-generated triage. Always consult a veterinarian."));
        }

        // Ambiguous / Other — low confidence triggers uncertain triage
        return Task.FromResult<MessageTriageResult?>(new MessageTriageResult(
            "Other",
            0.50,
            ["Thank you for your message. A staff member will review it shortly."],
            "This is an AI-generated triage. Always consult a veterinarian."));
    }

    public Task<string[]> GenerateSuggestedRepliesAsync(
        string conversationSubject,
        string lastOwnerMessage,
        string category,
        CancellationToken cancellationToken = default)
    {
        var replies = category switch
        {
            "MedicalUrgency" => new[]
            {
                "We understand your concern. Please bring your pet to the clinic immediately.",
                "This sounds urgent. Can you describe any additional symptoms?"
            },
            "MedicalQuestion" => new[]
            {
                "Thank you for reaching out. Based on your description, we recommend scheduling an examination.",
                "Please monitor your pet and note any changes in behavior or appetite.",
                "If symptoms worsen, please contact us immediately or visit the clinic."
            },
            "PostOperativeFollowUp" => new[]
            {
                "Thank you for the update. Please send a photo of the affected area if possible.",
                "We recommend scheduling a follow-up visit for proper examination."
            },
            "AppointmentRequest" => new[]
            {
                "We have availability this week. Would Sunday or Monday work for you?",
                "We would be happy to schedule an appointment. What time works best?"
            },
            _ => new[]
            {
                "Thank you for your message. How can we assist you further?",
                "We have received your message and will respond shortly."
            }
        };

        return Task.FromResult(replies);
    }

    private static bool ContainsAny(string text, string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
}
