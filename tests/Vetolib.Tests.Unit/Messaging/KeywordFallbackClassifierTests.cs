using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Vetolib.Messaging.Application.Services;
using Vetolib.Messaging.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class KeywordFallbackClassifierTests
{
    private readonly KeywordFallbackClassifier _classifier = new(NullLogger<KeywordFallbackClassifier>.Instance);

    [Theory]
    [InlineData("My cat is dying, please help!")]
    [InlineData("Emergency - dog has been poisoned")]
    [InlineData("My pet is having a seizure")]
    [InlineData("Dog collapsed and not breathing")]
    [InlineData("There is blood everywhere")]
    public async Task ClassifyAsync_UrgentEnglishKeyword_ReturnsCriticalMedical(string message)
    {
        var result = await _classifier.ClassifyAsync(message, null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Critical);
        result.Category.Should().Be(ClassifiedCategory.MedicalConcern);
        result.Confidence.Should().BeGreaterOrEqualTo(0.7);
    }

    [Theory]
    [InlineData("\u0637\u0648\u0627\u0631\u0626 \u0627\u0644\u062d\u064a\u0648\u0627\u0646")]          // طوارئ الحيوان (animal emergency)
    [InlineData("\u0627\u0644\u0642\u0637 \u064a\u0639\u0627\u0646\u064a \u0645\u0646 \u0646\u0632\u064a\u0641")] // القط يعاني من نزيف (cat is bleeding)
    [InlineData("\u062a\u0633\u0645\u0645 \u0627\u0644\u0643\u0644\u0628")]                              // تسمم الكلب (dog poisoning)
    [InlineData("\u0625\u063a\u0645\u0627\u0621 \u0627\u0644\u062d\u064a\u0648\u0627\u0646")]            // إغماء الحيوان (animal fainting)
    public async Task ClassifyAsync_UrgentArabicKeyword_ReturnsCriticalMedical(string message)
    {
        var result = await _classifier.ClassifyAsync(message, null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Critical);
        result.Category.Should().Be(ClassifiedCategory.MedicalConcern);
    }

    [Fact]
    public async Task ClassifyAsync_MedicalKeyword_ReturnsNormalMedical()
    {
        var result = await _classifier.ClassifyAsync("I need a vaccination for my puppy", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Normal);
        result.Category.Should().Be(ClassifiedCategory.MedicalConcern);
    }

    [Fact]
    public async Task ClassifyAsync_AppointmentKeyword_ReturnsAppointmentRequest()
    {
        var result = await _classifier.ClassifyAsync("I would like to schedule an appointment", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Normal);
        result.Category.Should().Be(ClassifiedCategory.AppointmentRequest);
    }

    [Fact]
    public async Task ClassifyAsync_AdminKeyword_ReturnsAdministrative()
    {
        var result = await _classifier.ClassifyAsync("Can you send me the invoice for last visit?", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Low);
        result.Category.Should().Be(ClassifiedCategory.AdministrativeRequest);
    }

    [Fact]
    public async Task ClassifyAsync_FeedbackKeyword_ReturnsFeedback()
    {
        var result = await _classifier.ClassifyAsync("Thank you for the excellent service", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Low);
        result.Category.Should().Be(ClassifiedCategory.Feedback);
    }

    [Fact]
    public async Task ClassifyAsync_NoKeywords_ReturnsNormalUnspecifiedWithLowConfidence()
    {
        var result = await _classifier.ClassifyAsync("Hello, how are you doing today?", null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Normal);
        result.Category.Should().Be(ClassifiedCategory.Unspecified);
        result.Confidence.Should().Be(0.3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ClassifyAsync_EmptyMessage_ReturnsNormalUnspecified(string? message)
    {
        var result = await _classifier.ClassifyAsync(message!, null);

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Normal);
        result.Category.Should().Be(ClassifiedCategory.Unspecified);
        result.Confidence.Should().Be(0.1);
    }

    [Fact]
    public async Task ClassifyAsync_UrgentKeywordInSubject_DetectsUrgency()
    {
        var result = await _classifier.ClassifyAsync(
            "Please help",
            "Emergency - dog emergency");

        result.Should().NotBeNull();
        result!.Urgency.Should().Be(ClassifiedUrgency.Critical);
    }
}
