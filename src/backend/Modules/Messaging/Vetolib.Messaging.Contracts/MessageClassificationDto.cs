namespace Vetolib.Messaging.Contracts;

public record MessageClassificationDto(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category,
    double Confidence,
    bool IsFlaggedForReview,
    Guid? OverriddenByUserId,
    ClassifiedUrgency? OriginalAiUrgency,
    ClassifiedCategory? OriginalAiCategory
);
