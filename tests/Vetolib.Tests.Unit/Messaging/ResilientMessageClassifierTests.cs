using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class ResilientMessageClassifierTests
{
    private readonly IMessageClassifier _innerClassifier = Substitute.For<IMessageClassifier>();
    private readonly KeywordFallbackClassifier _fallback = new(NullLogger<KeywordFallbackClassifier>.Instance);
    private readonly ResilientMessageClassifier _sut;

    public ResilientMessageClassifierTests()
    {
        _sut = new ResilientMessageClassifier(
            _innerClassifier,
            _fallback,
            NullLogger<ResilientMessageClassifier>.Instance);
    }

    [Fact]
    public async Task ClassifyAsync_WhenAISucceeds_ReturnsAIResult()
    {
        var expected = new MessageClassificationResult(
            ClassifiedUrgency.High,
            ClassifiedCategory.MedicalConcern,
            0.95);

        _innerClassifier
            .ClassifyAsync("test message", null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.ClassifyAsync("test message", null);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task ClassifyAsync_WhenAIThrows_FallsBackToKeywordClassifier()
    {
        _innerClassifier
            .ClassifyAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("AI service unavailable"));

        var result = await _sut.ClassifyAsync("My dog is dying, emergency!", null);

        result.Should().NotBeNull();
        // Keyword fallback should detect "dying" and "emergency"
        result!.Urgency.Should().Be(ClassifiedUrgency.Critical);
        result.Category.Should().Be(ClassifiedCategory.MedicalConcern);
    }

    [Fact]
    public async Task ClassifyAsync_WhenAIThrows_NonUrgentMessage_FallsBackWithDefaultClassification()
    {
        _innerClassifier
            .ClassifyAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("AI service unavailable"));

        var result = await _sut.ClassifyAsync("Hello, just a general question", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Normal);
        result.Category.Should().Be(ClassifiedCategory.Unspecified);
        result.Confidence.Should().Be(0.3);
    }

    [Fact]
    public async Task ClassifyAsync_WhenAIReturnsNull_ReturnsNull()
    {
        _innerClassifier
            .ClassifyAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((MessageClassificationResult?)null);

        var result = await _sut.ClassifyAsync("test message", null);

        result.Should().BeNull();
    }
}
