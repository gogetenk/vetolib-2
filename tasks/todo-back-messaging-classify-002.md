# todo-back-messaging-classify-002 -- Domain: enrichir Message entity + migration EF

**Module** : Messaging
**Priorite** : critique
**Skills** : `ardalis-result`, `multitenant-efcore`, `ardalis-modular-monolith`
**Feature** : `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageClassification.feature`
**Branche** : `feat/messaging-classification-domain`
**Depend de** : todo-back-messaging-classify-001

## Contexte

La Message entity doit stocker le resultat de la classification AI et permettre un override veterinaire. Les champs AI originaux sont preserves meme apres override (feature scenario : "the original AI classification should be preserved for training purposes").

## Travail demande

### 1. Enrichir l'entite Message (Domain/Message.cs)

Ajouter les proprietes suivantes :

```csharp
// Classification AI
public ClassifiedUrgency? ClassifiedUrgency { get; private set; }
public ClassifiedCategory? ClassifiedCategory { get; private set; }
public double? ClassifiedConfidence { get; private set; }
public bool IsFlaggedForReview { get; private set; }

// Override vet
public Guid? OverriddenByUserId { get; private set; }
public ClassifiedUrgency? OriginalAiUrgency { get; private set; }
public ClassifiedCategory? OriginalAiCategory { get; private set; }

// Feedback
public bool? ClassificationFeedbackCorrect { get; private set; }
```

### 2. Methodes domain sur Message

```csharp
/// <summary>
/// Applies AI classification result. Called once when message is created.
/// </summary>
public Result ApplyClassification(
    ClassifiedUrgency urgency,
    ClassifiedCategory category,
    double confidence,
    bool flagForReview)
{
    if (confidence is < 0 or > 1)
        return Result.Error("INVALID_CONFIDENCE:Confidence must be between 0 and 1");

    ClassifiedUrgency = urgency;
    ClassifiedCategory = category;
    ClassifiedConfidence = confidence;
    IsFlaggedForReview = flagForReview;
    return Result.Success();
}

/// <summary>
/// Vet overrides the AI classification. Original AI values are preserved.
/// </summary>
public Result OverrideClassification(
    Guid userId,
    ClassifiedUrgency newUrgency,
    ClassifiedCategory newCategory)
{
    if (ClassifiedUrgency is null)
        return Result.Error("NOT_CLASSIFIED:Message has not been classified yet");

    // Preserve original AI values (only on first override)
    if (OriginalAiUrgency is null)
    {
        OriginalAiUrgency = ClassifiedUrgency;
        OriginalAiCategory = ClassifiedCategory;
    }

    OverriddenByUserId = userId;
    ClassifiedUrgency = newUrgency;
    ClassifiedCategory = newCategory;
    IsFlaggedForReview = false; // override resolves review flag
    return Result.Success();
}

/// <summary>
/// Staff confirms or corrects the classification (feedback for training).
/// </summary>
public Result RecordClassificationFeedback(bool isCorrect)
{
    if (ClassifiedUrgency is null)
        return Result.Error("NOT_CLASSIFIED:Message has not been classified yet");

    ClassificationFeedbackCorrect = isCorrect;
    return Result.Success();
}
```

### 3. Mettre a jour ToDto() sur Message

```csharp
public MessageDto ToDto() => new(
    Id, ConversationId, Sender, SenderUserId, Body, IsInternalNote, SentAt,
    ClassifiedUrgency is not null
        ? new MessageClassificationDto(
            ClassifiedUrgency.Value,
            ClassifiedCategory!.Value,
            ClassifiedConfidence!.Value,
            IsFlaggedForReview,
            OverriddenByUserId,
            OriginalAiUrgency,
            OriginalAiCategory)
        : null
);
```

### 4. EF Configuration (MessageConfiguration.cs)

Ajouter dans `Configure()` :

```csharp
builder.Property(m => m.ClassifiedUrgency)
    .HasConversion<string>();

builder.Property(m => m.ClassifiedCategory)
    .HasConversion<string>();

builder.Property(m => m.ClassifiedConfidence)
    .HasPrecision(5, 4);

builder.Property(m => m.OriginalAiUrgency)
    .HasConversion<string>();

builder.Property(m => m.OriginalAiCategory)
    .HasConversion<string>();

// Index for stats queries (accuracy report)
builder.HasIndex(m => m.ClassifiedCategory);
```

### 5. Migration EF

Generer avec timestamp `20260322120000` :

```bash
dotnet ef migrations add AddMessageClassification \
  --project src/backend/Modules/Messaging/Vetolib.Messaging \
  --startup-project src/backend/Vetolib.Api \
  --context MessagingDbContext
```

Colonnes ajoutees a la table `messages` (schema `messaging`) :
- `ClassifiedUrgency` text nullable
- `ClassifiedCategory` text nullable
- `ClassifiedConfidence` numeric(5,4) nullable
- `IsFlaggedForReview` boolean default false
- `OverriddenByUserId` uuid nullable
- `OriginalAiUrgency` text nullable
- `OriginalAiCategory` text nullable
- `ClassificationFeedbackCorrect` boolean nullable

## Criteres

- [ ] 8 nouvelles colonnes dans la migration
- [ ] Methodes `ApplyClassification`, `OverrideClassification`, `RecordClassificationFeedback` retournent `Result`
- [ ] L'override preserve les valeurs AI originales
- [ ] `dotnet build` passe
- [ ] `dotnet ef migrations script` genere le SQL correct
