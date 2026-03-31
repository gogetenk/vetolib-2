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

        // Default language (En) should not have Arabic fields
        dto.SubjectiveAr.Should().BeNull();
        dto.ObjectiveAr.Should().BeNull();
        dto.AssessmentAr.Should().BeNull();
        dto.PlanAr.Should().BeNull();
        dto.SummaryAr.Should().BeNull();
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
        result1.Value.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result2.Value.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateAsync_WithArabicLanguage_ReturnsArabicContent()
    {
        // Arrange
        var request = new SoapNoteRequest(
            Species: "Dog",
            Breed: "Golden Retriever",
            PatientName: "Buddy",
            Symptoms: "Limping",
            Vitals: "T: 38.5C",
            Diagnosis: "Sprain",
            TreatmentPlan: "Rest",
            Prescriptions: new List<string> { "Meloxicam" },
            Language: SoapLanguage.Ar);

        // Act
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        // Arabic content in primary fields
        dto.Subjective.Should().Contain("يُحضر المالك");
        dto.Subjective.Should().Contain("Buddy");
        dto.Objective.Should().Contain("نتائج الفحص السريري");
        dto.Assessment.Should().Contain("التقييم السريري");
        dto.Plan.Should().Contain("خطة العلاج");
        dto.Summary.Should().Contain("ملاحظات SOAP");

        // Arabic-specific fields should be null (single language mode)
        dto.SubjectiveAr.Should().BeNull();
    }

    [Fact]
    public async Task GenerateAsync_WithBothLanguages_ReturnsBilingualContent()
    {
        // Arrange
        var request = new SoapNoteRequest(
            Species: "Cat",
            Breed: "Siamese",
            PatientName: "Luna",
            Symptoms: "Sneezing",
            Vitals: "T: 38.0C",
            Diagnosis: "Upper respiratory infection",
            TreatmentPlan: "Antibiotics",
            Prescriptions: new List<string> { "Amoxicillin" },
            Language: SoapLanguage.Both);

        // Act
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        // English primary fields
        dto.Subjective.Should().Contain("Owner presents Luna");
        dto.Subjective.Should().Contain("Cat");
        dto.Objective.Should().Contain("Physical examination findings");
        dto.Assessment.Should().Contain("Clinical assessment");
        dto.Plan.Should().Contain("Treatment plan");
        dto.Summary.Should().Contain("SOAP note for Luna");

        // Arabic secondary fields
        dto.SubjectiveAr.Should().NotBeNullOrWhiteSpace();
        dto.SubjectiveAr.Should().Contain("يُحضر المالك");
        dto.SubjectiveAr.Should().Contain("Luna");
        dto.ObjectiveAr.Should().Contain("نتائج الفحص السريري");
        dto.AssessmentAr.Should().Contain("التقييم السريري");
        dto.PlanAr.Should().Contain("خطة العلاج");
        dto.SummaryAr.Should().Contain("ملاحظات SOAP");
    }

    [Fact]
    public async Task GenerateAsync_WithArabicLanguage_MinimalRequest_ReturnsArabicDefaults()
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
            Prescriptions: new List<string>(),
            Language: SoapLanguage.Ar);

        // Act
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Objective.Should().Contain("ضمن الحدود الطبيعية");
        dto.Assessment.Should().Contain("يُوصى بإجراء فحوصات إضافية");
        dto.Plan.Should().Contain("لم يتم وصف أي أدوية");
    }

    [Fact]
    public async Task GenerateAsync_DefaultLanguage_IsEnglish()
    {
        // Arrange — no Language parameter, defaults to En
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
        var result = await _sut.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Subjective.Should().Contain("Owner presents");
        dto.SubjectiveAr.Should().BeNull();
    }
}
