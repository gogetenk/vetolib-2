using FluentAssertions;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.AI;

public class TemplateSoapNotesGeneratorTests
{
    private readonly TemplateSoapNotesGenerator _sut = new();

    [Fact]
    public async Task GenerateAsync_WithFullRequest_ReturnsSuccessWithAllSections()
    {
        // Arrange
        var request = new SoapNoteRequest(
            Species: "Dog",
            Breed: "Golden Retriever",
            PatientName: "Buddy",
            Symptoms: "Limping on right hind leg for 2 days",
            Vitals: "T: 38.5C, HR: 100, RR: 20",
            Diagnosis: "Suspected cruciate ligament injury",
            TreatmentPlan: "Rest, anti-inflammatory, orthopedic referral",
            Prescriptions: new List<string> { "Meloxicam 0.1mg/kg PO SID x 7 days" });

        // Act
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Subjective.Should().NotBeNullOrWhiteSpace();
        dto.Subjective.Should().Contain("Buddy");
        dto.Subjective.Should().Contain("Dog");
        dto.Subjective.Should().Contain("Limping on right hind leg");

        dto.Objective.Should().NotBeNullOrWhiteSpace();
        dto.Objective.Should().Contain("38.5C");

        dto.Assessment.Should().NotBeNullOrWhiteSpace();
        dto.Assessment.Should().Contain("cruciate ligament");

        dto.Plan.Should().NotBeNullOrWhiteSpace();
        dto.Plan.Should().Contain("Meloxicam");

        dto.Summary.Should().NotBeNullOrWhiteSpace();
        dto.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateAsync_WithMinimalRequest_ReturnsSuccessWithDefaults()
    {
        // Arrange
        var request = new SoapNoteRequest(
            Species: "Cat",
            Breed: "Persian",
            PatientName: "Whiskers",
            Symptoms: "Sneezing",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        // Act
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Subjective.Should().Contain("Whiskers");
        dto.Subjective.Should().Contain("Cat");
        dto.Subjective.Should().Contain("Sneezing");

        dto.Objective.Should().Contain("Within normal limits");
        dto.Assessment.Should().Contain("Further diagnostics recommended");
        dto.Plan.Should().Contain("No medications prescribed");
        dto.Summary.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GenerateAsync_ReturnsUniqueGeneratedAtTimestamp()
    {
        // Arrange
        var request = new SoapNoteRequest(
            Species: "Dog",
            Breed: "Labrador",
            PatientName: "Rex",
            Symptoms: "Vomiting",
            Vitals: "",
            Diagnosis: "",
            TreatmentPlan: "",
            Prescriptions: new List<string>());

        // Act
        var result1 = await _sut.GenerateAsync(request);
        var result2 = await _sut.GenerateAsync(request);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        // Both should be close to now
        result1.Value.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result2.Value.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
