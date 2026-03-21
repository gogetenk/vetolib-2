using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

/// <summary>
/// Decorator that wraps an <see cref="ISoapNotesGenerator"/> (typically the AI-backed one)
/// with Polly Circuit Breaker, Retry and Timeout policies.
/// When the circuit is open, falls back to <see cref="TemplateSoapNotesGenerator"/>.
/// </summary>
internal sealed class ResilientSoapNotesGenerator : ISoapNotesGenerator
{
    private readonly ISoapNotesGenerator _inner;
    private readonly TemplateSoapNotesGenerator _fallback;
    private readonly ILogger<ResilientSoapNotesGenerator> _logger;
    private readonly ResiliencePipeline _pipeline;

    public ResilientSoapNotesGenerator(
        ISoapNotesGenerator inner,
        TemplateSoapNotesGenerator fallback,
        ILogger<ResilientSoapNotesGenerator> logger)
    {
        _inner = inner;
        _fallback = fallback;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            // Timeout: 30s for AI calls
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30)
            })
            // Retry: 2 retries with exponential backoff (1s, 2s)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
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
                    _logger.LogWarning("AI SOAP circuit breaker OPENED. Falling back to template generator for {Duration}s.",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    _logger.LogInformation("AI SOAP circuit breaker CLOSED. AI service resumed.");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<Result<SoapNoteDto>> GenerateAsync(
        SoapNoteRequest request,
        CancellationToken ct = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async token => await _inner.GenerateAsync(request, token),
                ct);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("AI circuit is open — using template fallback for SOAP note generation.");
            return await _fallback.GenerateAsync(request, ct);
        }
        catch (TimeoutRejectedException)
        {
            _logger.LogWarning("AI call timed out — using template fallback for SOAP note generation.");
            return await _fallback.GenerateAsync(request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI SOAP generation failed after retries — using template fallback.");
            return await _fallback.GenerateAsync(request, ct);
        }
    }
}
