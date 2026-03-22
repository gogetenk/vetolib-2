# todo-back-messaging-classify-004 -- Handlers: classifier integration + override endpoint + feedback

**Module** : Messaging
**Priorite** : critique
**Skills** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`
**Feature** : `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageClassification.feature`
**Branche** : `feat/messaging-classification-handlers`
**Depend de** : todo-back-messaging-classify-001, 002, 003

## Contexte

Cette tache integre le classifier dans le flux existant et ajoute les endpoints d'override et de feedback.

## Travail demande

### 1. Modifier le flux d'ajout de message

Dans `CreateOwnerConversationHandler` et `SendOwnerMessageHandler`, apres l'ajout du message et avant le `SaveChangesAsync` :

```csharp
// Classify the message (in addition to conversation triage)
var classificationResult = await _messageClassifier.ClassifyAsync(
    request.Body,
    conversation.Subject,
    cancellationToken);

if (classificationResult is not null)
{
    var flagForReview = classificationResult.Confidence < 0.7
                     || classificationResult.Category == ClassifiedCategory.Unspecified;

    message.ApplyClassification(
        classificationResult.Urgency,
        classificationResult.Category,
        classificationResult.Confidence,
        flagForReview);

    // Publish event for critical/high urgency messages
    if (classificationResult.Urgency is ClassifiedUrgency.Critical or ClassifiedUrgency.High)
    {
        var preview = request.Body.Length > 100 ? request.Body[..100] + "..." : request.Body;
        await _mediator.Publish(new UrgentMessageClassifiedEvent(
            message.Id, conversation.Id, conversation.ClinicId,
            classificationResult.Urgency, preview), cancellationToken);
    }
}
```

Injecter `IMessageClassifier` dans les deux handlers via constructeur.

### 2. Nouveau command : OverrideMessageClassification

```
Application/Commands/OverrideMessageClassification/
  OverrideMessageClassificationCommand.cs
  OverrideMessageClassificationHandler.cs
  OverrideMessageClassificationValidator.cs
```

```csharp
public record OverrideMessageClassificationCommand(
    Guid ConversationId,
    Guid MessageId,
    ClassifiedUrgency Urgency,
    ClassifiedCategory Category
) : IRequest<Result<MessageDto>>;
```

Handler :
- Charger la conversation + le message
- Verifier que le message appartient a la conversation
- Appeler `message.OverrideClassification(userId, urgency, category)`
- Si la nouvelle categorie est "MedicalConcern" et l'ancienne ne l'etait pas, re-router la conversation vers le vet queue
- Sauvegarder

### 3. Nouveau command : RecordClassificationFeedback

```
Application/Commands/RecordClassificationFeedback/
  RecordClassificationFeedbackCommand.cs
  RecordClassificationFeedbackHandler.cs
  RecordClassificationFeedbackValidator.cs
```

```csharp
public record RecordClassificationFeedbackCommand(
    Guid ConversationId,
    Guid MessageId,
    bool IsCorrect
) : IRequest<Result>;
```

### 4. Nouvelle query : GetClassificationAccuracy

```
Application/Queries/GetClassificationAccuracy/
  GetClassificationAccuracyQuery.cs
  GetClassificationAccuracyHandler.cs
```

```csharp
public record GetClassificationAccuracyQuery(int MonthsBack = 1) : IRequest<Result<ClassificationAccuracyDto>>;
```

Handler :
- Compter les messages avec `ClassificationFeedbackCorrect != null`
- Taux = messages ou feedback = true / total
- Lister les top corrections (OriginalAiCategory -> ClassifiedCategory quand OverriddenByUserId != null)

### 5. Endpoints (MessagingEndpoints.cs)

Ajouter dans `MapMessagingApiEndpoints()` :

```csharp
// PATCH /conversations/{id}/messages/{messageId}/classify — vet override
group.MapPatch("/conversations/{id:guid}/messages/{messageId:guid}/classify", async (
    Guid id,
    Guid messageId,
    ClassifyMessageOverrideRequest request,
    ISender sender,
    CancellationToken ct) =>
{
    var cmd = new OverrideMessageClassificationCommand(id, messageId, request.Urgency, request.Category);
    return (await sender.Send(cmd, ct)).ToMinimalApiResult();
}).RequireAuthorization(policy => policy.RequireRole("Vet", "Admin"))
  .WithName("OverrideMessageClassification");

// POST /conversations/{id}/messages/{messageId}/classify/feedback — staff feedback
group.MapPost("/conversations/{id:guid}/messages/{messageId:guid}/classify/feedback", async (
    Guid id,
    Guid messageId,
    ClassificationFeedbackRequest request,
    ISender sender,
    CancellationToken ct) =>
{
    var cmd = new RecordClassificationFeedbackCommand(id, messageId, request.IsCorrect);
    return (await sender.Send(cmd, ct)).ToMinimalApiResult();
}).RequireAuthorization("ClinicStaff")
  .WithName("RecordClassificationFeedback");

// GET /stats/classification-accuracy — accuracy report
group.MapGet("/stats/classification-accuracy", async (
    int monthsBack,
    ISender sender,
    CancellationToken ct) =>
    (await sender.Send(new GetClassificationAccuracyQuery(monthsBack), ct)).ToMinimalApiResult())
    .RequireAuthorization(policy => policy.RequireRole("Admin"))
    .WithName("GetClassificationAccuracy");
```

### 6. Event handler : UrgentMessageClassifiedEventHandler

Notifier les veterinaires de garde (meme pattern que EmergencyEscalationBackgroundService mais immediat, pas polling).

```csharp
internal sealed class UrgentMessageClassifiedEventHandler : INotificationHandler<UrgentMessageClassifiedEvent>
{
    // Publier via MassTransit ou SSE broadcaster existant
    // Le scenario exige : "all on-duty veterinarians should receive a notification within 1 minute"
}
```

## Criteres

- [ ] `IMessageClassifier` injecte dans CreateOwnerConversationHandler et SendOwnerMessageHandler
- [ ] Le message est classifie AVANT le SaveChangesAsync
- [ ] Endpoint PATCH `/conversations/{id}/messages/{messageId}/classify` fonctionne
- [ ] L'override preserve les valeurs AI originales
- [ ] L'endpoint feedback enregistre isCorrect sur le message
- [ ] Le rapport d'accuracy calcule correctement le taux
- [ ] Les messages Critical/High declenchent UrgentMessageClassifiedEvent
- [ ] Tous les handlers retournent `Result<T>`
- [ ] `dotnet build` passe
