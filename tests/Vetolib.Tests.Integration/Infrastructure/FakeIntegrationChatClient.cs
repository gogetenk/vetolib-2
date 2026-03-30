using Microsoft.Extensions.AI;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Fake IChatClient for integration tests. Returns deterministic responses.
/// </summary>
public sealed class FakeIntegrationChatClient : IChatClient
{
    private bool _shouldThrow;

    public void SetShouldThrow(bool value) => _shouldThrow = value;

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_shouldThrow)
            throw new InvalidOperationException("Fake AI error");

        var json = """
            {
              "severity": "Normal",
              "estimatedDurationMinutes": 30,
              "recommendedSpecialty": "general practitioner",
              "reasoning": "Symptoms suggest a non-emergency condition requiring prompt veterinary attention.",
              "confidence": 0.85
            }
            """;
        var message = new ChatMessage(ChatRole.Assistant, json);
        var response = new ChatResponse(message);
        return Task.FromResult(response);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Streaming not supported in tests.");

    public object? GetService(Type serviceType, object? key = null)
        => serviceType == typeof(FakeIntegrationChatClient) ? this : null;

    public void Dispose() { }
}
