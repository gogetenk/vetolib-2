using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class ResilientSoapNotesGeneratorTests
{
    private static readonly SoapNoteRequest TestRequest = new(
        PatientName: "Buddy",
        Species: "Dog",
        Breed: "Labrador",
        Symptoms: "Vomiting for 2 days",
        Vitals: "T:39.2 HR:120 RR:24",
        Diagnosis: "Gastritis",
        TreatmentPlan: "Fluids + anti-emetics",
        Prescriptions: ["Cerenia 1mg/kg SQ SID x 3d"]);

    private static readonly SoapNoteDto ExpectedAiDto = new(
        Subjective: "AI Subjective",
        Objective: "AI Objective",
        Assessment: "AI Assessment",
        Plan: "AI Plan",
        Summary: "AI Summary",
        GeneratedAt: DateTime.UtcNow);

    private readonly ISoapNotesGenerator _innerMock;
    private readonly TemplateSoapNotesGenerator _fallback;
    private readonly ILogger<ResilientSoapNotesGenerator> _logger;

    public ResilientSoapNotesGeneratorTests()
    {
        _innerMock = Substitute.For<ISoapNotesGenerator>();
        _fallback = new TemplateSoapNotesGenerator();
        _logger = NullLogger<ResilientSoapNotesGenerator>.Instance;
    }

    private ResilientSoapNotesGenerator CreateSut() =>
        new(_innerMock, _fallback, _logger);

    [Fact]
    public async Task GenerateAsync_WhenAiSucceeds_ReturnsAiResult()
    {
        _innerMock.GenerateAsync(Arg.Any<SoapNoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<SoapNoteDto>.Success(ExpectedAiDto));

        var sut = CreateSut();
        var result = await sut.GenerateAsync(TestRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value.Subjective.Should().Be("AI Subjective");
    }

    [Fact]
    public async Task GenerateAsync_WhenAiFails_FallsBackToTemplate()
    {
        _innerMock.GenerateAsync(Arg.Any<SoapNoteRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("AI service down"));

        var sut = CreateSut();
        var result = await sut.GenerateAsync(TestRequest);

        result.IsSuccess.Should().BeTrue();
        result.Value.Subjective.Should().Contain("Buddy");
        result.Value.Subjective.Should().Contain("Dog");
    }

    [Fact]
    public async Task GenerateAsync_AfterMultipleFailures_CircuitOpensAndFallsBack()
    {
        // The circuit breaker needs 3 failures within sampling window to open
        _innerMock.GenerateAsync(Arg.Any<SoapNoteRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("AI service down"));

        var sut = CreateSut();

        // First 3 calls fail (triggers circuit opening via retry exhaustion + circuit breaker)
        for (var i = 0; i < 3; i++)
        {
            var result = await sut.GenerateAsync(TestRequest);
            result.IsSuccess.Should().BeTrue("fallback should succeed even when AI fails");
            result.Value.Subjective.Should().Contain("Buddy");
        }

        // Subsequent call should still return template fallback (circuit is now open)
        var finalResult = await sut.GenerateAsync(TestRequest);
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Subjective.Should().Contain("Buddy");
    }
}
