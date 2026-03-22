using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Classifies veterinary clinic messages using an LLM via <see cref="IChatClient"/>.
/// Supports English and Arabic message content.
/// </summary>
internal sealed class ClaudeMessageClassifier : IMessageClassifier
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt = """
        You are an expert veterinary clinic message classifier.
        Your role is to classify incoming messages from pet owners by urgency and category.

        You must handle messages in both English and Arabic.

        URGENCY LEVELS:
        - Low: general questions, feedback, non-time-sensitive requests
        - Normal: routine appointment requests, billing questions, follow-up inquiries
        - High: medical concerns that need attention within hours (vomiting, limping, refusal to eat)
        - Critical: life-threatening emergencies (poisoning, severe bleeding, difficulty breathing, seizures, collapse)

        CATEGORIES:
        - MedicalConcern: health-related questions or symptoms
        - AdministrativeRequest: billing, invoices, account-related inquiries
        - AppointmentRequest: scheduling, rescheduling, or cancelling appointments
        - PostOperativeFollowUp: questions about post-surgery recovery or care
        - Feedback: reviews, compliments, complaints about service
        - Other: anything that doesn't fit the above categories

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "urgency": "Low|Normal|High|Critical",
          "category": "MedicalConcern|AdministrativeRequest|AppointmentRequest|PostOperativeFollowUp|Feedback|Other",
          "confidence": 0.0-1.0
        }

        Do not include any text outside the JSON object.
        """;

    private readonly IChatClient _chatClient;
    private readonly ILogger<ClaudeMessageClassifier> _logger;

    public ClaudeMessageClassifier(
        IChatClient chatClient,
        ILogger<ClaudeMessageClassifier> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return new MessageClassificationResult(ClassifiedUrgency.Normal, ClassifiedCategory.Unspecified, 0.0);

        var userMessage = string.IsNullOrWhiteSpace(conversationSubject)
            ? $"Message: {messageText}"
            : $"Subject: {conversationSubject}\nMessage: {messageText}";

        ChatResponse completion;
        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt),
                new(ChatRole.User, userMessage)
            };

            var options = new ChatOptions
            {
                Temperature = 0.1f,
                ResponseFormat = ChatResponseFormat.Json
            };

            completion = await _chatClient.GetResponseAsync(messages, options, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI service call failed for message classification.");
            throw;
        }

        var rawResponse = completion.Text ?? string.Empty;
        ClassificationAiResponse? parsed;

        try
        {
            parsed = JsonSerializer.Deserialize<ClassificationAiResponse>(rawResponse, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI classification response: {Response}", rawResponse);
            return null;
        }

        if (parsed is null)
        {
            _logger.LogError("AI classification response was null: {Response}", rawResponse);
            return null;
        }

        var urgency = ParseUrgency(parsed.Urgency);
        var category = ParseCategory(parsed.Category);
        var confidence = Math.Clamp(parsed.Confidence, 0.0, 1.0);

        return new MessageClassificationResult(urgency, category, confidence);
    }

    private static ClassifiedUrgency ParseUrgency(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ClassifiedUrgency.Normal;

        return Enum.TryParse<ClassifiedUrgency>(value, ignoreCase: true, out var result)
            ? result
            : ClassifiedUrgency.Normal;
    }

    private static ClassifiedCategory ParseCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ClassifiedCategory.Unspecified;

        return Enum.TryParse<ClassifiedCategory>(value, ignoreCase: true, out var result)
            ? result
            : ClassifiedCategory.Unspecified;
    }

    private sealed record ClassificationAiResponse(
        string? Urgency,
        string? Category,
        double Confidence);
}
