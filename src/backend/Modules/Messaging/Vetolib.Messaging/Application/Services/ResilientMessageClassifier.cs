using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Decorator that wraps <see cref="ClaudeMessageClassifier"/> with Polly Circuit Breaker,
/// Retry and Timeout policies. When the circuit is open or AI fails, falls back to
/// <see cref="KeywordFallbackClassifier"/>.
/// </summary>
internal sealed class ResilientMessageClassifier : IMessageClassifier
{
    private readonly IMessageClassifier _inner;
    private readonly KeywordFallbackClassifier _fallback;
    private readonly ILogger<ResilientMessageClassifier> _logger;
    private readonly ResiliencePipeline _pipeline;

    public ResilientMessageClassifier(
        IMessageClassifier inner,
        KeywordFallbackClassifier fallback,
        ILogger<ResilientMessageClassifier> logger)
    {
        _inner = inner;
        _fallback = fallback;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            // Timeout: 15s for classification calls (lighter than SOAP generation)
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(15)
            })
            // Retry: 1 retry with 1s delay
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(ex => ex is not OperationCanceledException and not BrokenCircuitException)
            })
            // Circuit Breaker: opens after 3 failures, stays open 30s
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(ex => ex is not OperationCanceledException),
                OnOpened = args =>
                {
                    _logger.LogWarning(
                        "Message classification circuit breaker OPENED. Falling back to keyword classifier for {Duration}s.",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    _logger.LogInformation("Message classification circuit breaker CLOSED. AI service resumed.");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async token => await _inner.ClassifyAsync(messageText, conversationSubject, token),
                cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Classification circuit is open - using keyword fallback.");
            return await _fallback.ClassifyAsync(messageText, conversationSubject, cancellationToken);
        }
        catch (TimeoutRejectedException)
        {
            _logger.LogWarning("Classification AI call timed out - using keyword fallback.");
            return await _fallback.ClassifyAsync(messageText, conversationSubject, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Message classification failed after retries - using keyword fallback.");
            return await _fallback.ClassifyAsync(messageText, conversationSubject, cancellationToken);
        }
    }
}
