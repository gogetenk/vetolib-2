using Microsoft.Extensions.AI;

namespace Vetolib.Tests.Acceptance.Support;

/// <summary>
/// A fake IChatClient for acceptance tests that returns configurable JSON responses
/// without requiring a real LLM service.
/// </summary>
internal class FakeChatClient : IChatClient
{
    private string _nextResponse;
    private bool _shouldThrow;

    public FakeChatClient(string defaultResponse = """
        {
          "severity": "Normal",
          "estimatedDurationMinutes": 30,
          "recommendedSpecialty": "general practitioner",
          "reasoning": "Pet shows moderate symptoms requiring veterinary attention but no immediate danger.",
          "confidence": 0.85
        }
        """)
    {
        _nextResponse = defaultResponse;
    }

    public void SetNextResponse(string jsonResponse) => _nextResponse = jsonResponse;

    public void SetShouldThrow(bool shouldThrow) => _shouldThrow = shouldThrow;

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_shouldThrow)
            throw new HttpRequestException("Simulated AI service unavailable");

        // Detect emergency keywords in user message for emergency scenario
        var userMessage = chatMessages
            .LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var response = _nextResponse;

        if (userMessage.Contains("chocolate", StringComparison.OrdinalIgnoreCase) ||
            userMessage.Contains("trembling", StringComparison.OrdinalIgnoreCase))
        {
            response = """
                {
                  "severity": "Emergency",
                  "estimatedDurationMinutes": 60,
                  "recommendedSpecialty": "emergency",
                  "reasoning": "Chocolate ingestion in dogs is toxic and can be life-threatening. Trembling indicates active neurological involvement.",
                  "confidence": 0.95
                }
                """;
        }

        var message = new ChatMessage(ChatRole.Assistant, response);
        var completion = new ChatResponse(message)
        {
            Usage = new UsageDetails
            {
                InputTokenCount = 42,
                OutputTokenCount = 88
            }
        };

        return Task.FromResult(completion);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Streaming is not supported in FakeChatClient.");

    public object? GetService(Type serviceType, object? key = null)
        => serviceType == typeof(FakeChatClient) ? this : null;

    public void Dispose() { }
}
