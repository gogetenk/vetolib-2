# todo-back-messaging-classify-001 -- Contracts: classification enums, DTOs, IMessageClassifier interface

**Module** : Messaging
**Priorite** : critique
**Skills** : `ardalis-result`, `ardalis-modular-monolith`
**Feature** : `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageClassification.feature`
**Branche** : `feat/messaging-classification-contracts`

## Contexte

La feature MessageClassification.feature introduit un concept nouveau : la classification **par message** (urgency + category + confidence), distinct du triage **par conversation** qui existe deja. Le triage conversation (ITriageOrchestrator) reste en place. La classification message est un enrichissement supplementaire.

## Travail demande

### 1. Nouveaux enums dans Messaging.Contracts

```csharp
// ClassifiedUrgency.cs
public enum ClassifiedUrgency
{
    Low,
    Normal,
    High,
    Critical
}

// ClassifiedCategory.cs — plus granulaire que MessageCategory existant
public enum ClassifiedCategory
{
    MedicalConcern,
    AdministrativeRequest,
    AppointmentRequest,
    PostOperativeFollowUp,
    Feedback,
    Unspecified,
    Other
}
```

### 2. DTOs dans Messaging.Contracts

```csharp
// MessageClassificationDto.cs
public record MessageClassificationDto(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category,
    double Confidence,
    bool IsFlaggedForReview,
    Guid? OverriddenByUserId,
    ClassifiedUrgency? OriginalAiUrgency,
    ClassifiedCategory? OriginalAiCategory
);

// ClassifyMessageOverrideRequest.cs
public record ClassifyMessageOverrideRequest(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category
);

// ClassificationFeedbackRequest.cs
public record ClassificationFeedbackRequest(
    bool IsCorrect
);

// ClassificationAccuracyDto.cs
public record ClassificationAccuracyDto(
    double AccuracyRate,
    int TotalClassified,
    int TotalCorrected,
    IReadOnlyList<CategoryCorrectionDto> MostCommonCorrections
);

public record CategoryCorrectionDto(
    ClassifiedCategory FromCategory,
    ClassifiedCategory ToCategory,
    int Count
);
```

### 3. Interface IMessageClassifier dans Messaging.Contracts

```csharp
// IMessageClassifier.cs
public interface IMessageClassifier
{
    /// <summary>
    /// Classifies a message text and returns urgency, category, and confidence.
    /// Returns null if the service is unavailable (circuit breaker open, AI down).
    /// </summary>
    Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default);
}

public record MessageClassificationResult(
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category,
    double Confidence
);
```

### 4. Nouveau domain event dans Messaging.Contracts/Events

```csharp
// UrgentMessageClassifiedEvent.cs
public record UrgentMessageClassifiedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid ClinicId,
    ClassifiedUrgency Urgency,
    string MessagePreview
) : MediatR.INotification;
```

### 5. Enrichir MessageDto

Ajouter les champs de classification au MessageDto existant. ATTENTION : cela casse le constructeur record existant, donc tous les appelants de `m.ToDto()` devront etre mis a jour (tache 003).

```csharp
public record MessageDto(
    Guid Id,
    Guid ConversationId,
    MessageSender Sender,
    Guid? SenderUserId,
    string Body,
    bool IsInternalNote,
    DateTime SentAt,
    // Classification fields (nullable — not all messages are classified)
    MessageClassificationDto? Classification
);
```

## Criteres

- [ ] Les 6 fichiers sont crees dans Messaging.Contracts
- [ ] Aucune reference au runtime Messaging
- [ ] `dotnet build` passe (les appelants de MessageDto casseront — attendu, corrige dans tache 003)
- [ ] Les enums correspondent exactement aux valeurs du .feature : Low, Normal, High, Critical / Medical concern, Administrative request, Appointment request, Post-operative follow-up, Unspecified
