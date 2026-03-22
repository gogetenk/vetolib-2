using FluentAssertions;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

/// <summary>
/// Tests for Message domain classification methods (ApplyClassification, OverrideClassification, RecordFeedback).
/// </summary>
public class MessageClassificationIntegrationTests
{
    private static readonly Guid ConversationId = Guid.NewGuid();

    [Fact]
    public void ApplyClassification_SetsAllFields()
    {
        var message = CreateMessage();

        var result = message.ApplyClassification(
            ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern, 0.92, true);

        result.IsSuccess.Should().BeTrue();
        message.ClassifiedUrgency.Should().Be(ClassifiedUrgency.High);
        message.ClassifiedCategory.Should().Be(ClassifiedCategory.MedicalConcern);
        message.ClassifiedConfidence.Should().Be(0.92);
        message.IsFlaggedForReview.Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void ApplyClassification_InvalidConfidence_ReturnsError(double confidence)
    {
        var message = CreateMessage();

        var result = message.ApplyClassification(
            ClassifiedUrgency.Normal, ClassifiedCategory.Other, confidence, false);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_CONFIDENCE"));
    }

    [Fact]
    public void OverrideClassification_PreservesOriginalAiValues()
    {
        var message = CreateMessage();
        message.ApplyClassification(ClassifiedUrgency.Low, ClassifiedCategory.Feedback, 0.7, false);

        var userId = Guid.NewGuid();
        var result = message.OverrideClassification(
            userId, ClassifiedUrgency.Critical, ClassifiedCategory.MedicalConcern);

        result.IsSuccess.Should().BeTrue();
        message.OriginalAiUrgency.Should().Be(ClassifiedUrgency.Low);
        message.OriginalAiCategory.Should().Be(ClassifiedCategory.Feedback);
        message.ClassifiedUrgency.Should().Be(ClassifiedUrgency.Critical);
        message.ClassifiedCategory.Should().Be(ClassifiedCategory.MedicalConcern);
        message.OverriddenByUserId.Should().Be(userId);
        message.IsFlaggedForReview.Should().BeFalse(); // override resolves review flag
    }

    [Fact]
    public void OverrideClassification_WhenNotClassified_ReturnsError()
    {
        var message = CreateMessage();

        var result = message.OverrideClassification(
            Guid.NewGuid(), ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("NOT_CLASSIFIED"));
    }

    [Fact]
    public void OverrideClassification_SecondOverride_DoesNotChangeOriginalAiValues()
    {
        var message = CreateMessage();
        message.ApplyClassification(ClassifiedUrgency.Low, ClassifiedCategory.Feedback, 0.7, false);

        var userId = Guid.NewGuid();
        message.OverrideClassification(userId, ClassifiedUrgency.High, ClassifiedCategory.AppointmentRequest);

        // Second override
        message.OverrideClassification(userId, ClassifiedUrgency.Critical, ClassifiedCategory.MedicalConcern);

        // Original AI values should still be from the first classification
        message.OriginalAiUrgency.Should().Be(ClassifiedUrgency.Low);
        message.OriginalAiCategory.Should().Be(ClassifiedCategory.Feedback);
        message.ClassifiedUrgency.Should().Be(ClassifiedUrgency.Critical);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RecordClassificationFeedback_WhenClassified_SetsFeedback(bool isCorrect)
    {
        var message = CreateMessage();
        message.ApplyClassification(ClassifiedUrgency.Normal, ClassifiedCategory.Other, 0.8, false);

        var result = message.RecordClassificationFeedback(isCorrect);

        result.IsSuccess.Should().BeTrue();
        message.ClassificationFeedbackCorrect.Should().Be(isCorrect);
    }

    [Fact]
    public void RecordClassificationFeedback_WhenNotClassified_ReturnsError()
    {
        var message = CreateMessage();

        var result = message.RecordClassificationFeedback(true);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("NOT_CLASSIFIED"));
    }

    [Fact]
    public void ToDto_WhenClassified_IncludesClassificationDto()
    {
        var message = CreateMessage();
        message.ApplyClassification(ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern, 0.9, true);

        var dto = message.ToDto();

        dto.Classification.Should().NotBeNull();
        dto.Classification!.Urgency.Should().Be(ClassifiedUrgency.High);
        dto.Classification.Category.Should().Be(ClassifiedCategory.MedicalConcern);
        dto.Classification.Confidence.Should().Be(0.9);
        dto.Classification.IsFlaggedForReview.Should().BeTrue();
    }

    [Fact]
    public void ToDto_WhenNotClassified_ClassificationIsNull()
    {
        var message = CreateMessage();

        var dto = message.ToDto();

        dto.Classification.Should().BeNull();
    }

    private static Message CreateMessage()
    {
        return Message.Create(ConversationId, MessageSender.Owner, null, "Test message body").Value;
    }
}
