# todo-back-messaging-classify-003 -- Implementation: ClaudeMessageClassifier + KeywordFallbackClassifier

**Module** : Messaging
**Priorite** : critique
**Skills** : `ardalis-result`, `ardalis-modular-monolith`
**Feature** : `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageClassification.feature`
**Branche** : `feat/messaging-classifier-impl`
**Depend de** : todo-back-messaging-classify-001

## Contexte

L'interface `IMessageClassifier` (definie dans Contracts, tache 001) a deux implementations :
1. `ClaudeMessageClassifier` — appelle l'API Claude pour classifier (primaire)
2. `KeywordFallbackClassifier` — fallback quand l'AI est down (circuit breaker)

Le circuit breaker existe deja dans le pattern utilise par `ResilientSoapNotesGenerator` dans le module AI. Suivre le meme pattern.

## Architecture de decision

Le classifier vit dans **Vetolib.Messaging** (pas dans Vetolib.AI) car :
- L'interface est dans Messaging.Contracts
- La logique fallback keyword est specifique au domaine messaging
- Le module AI fournit deja `IMessageTriageService` pour le triage conversation — c'est un service different
- Le classifier message peut appeler l'API Claude directement via HttpClient (pas besoin de passer par AI module)

## Travail demande

### 1. ClaudeMessageClassifier (Infrastructure/)

```csharp
internal sealed class ClaudeMessageClassifier : IMessageClassifier
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeMessageClassifier> _logger;

    // Structured prompt for classification
    private const string SystemPrompt = """
        You are a veterinary clinic message classifier. Classify the following pet owner message.

        Return a JSON object with exactly these fields:
        - urgency: one of "Low", "Normal", "High", "Critical"
        - category: one of "MedicalConcern", "AdministrativeRequest", "AppointmentRequest", "PostOperativeFollowUp", "Feedback", "Unspecified", "Other"
        - confidence: a number between 0 and 1

        Classification rules:
        - Critical: life-threatening symptoms (poisoning, heavy bleeding, collapse, difficulty breathing, seizures)
        - High: significant symptoms requiring attention within hours (not eating 2+ days, lethargy, vomiting, limping)
        - Normal: routine medical or scheduling concerns
        - Low: purely administrative (certificates, invoices, records)

        - Messages that are very short or ambiguous should be classified as High urgency with category "Unspecified"
        - The message may be in any language (English, Arabic, etc.) — classify based on content regardless of language

        Respond ONLY with the JSON object, no other text.
        """;

    public async Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default)
    {
        // Build prompt with context
        var userPrompt = conversationSubject is not null
            ? $"Subject: {conversationSubject}\nMessage: {messageText}"
            : $"Message: {messageText}";

        // Call Claude API, parse JSON response
        // Return null on any failure (circuit breaker / timeout / parse error)
    }
}
```

### 2. KeywordFallbackClassifier (Infrastructure/)

```csharp
internal sealed class KeywordFallbackClassifier : IMessageClassifier
{
    // Keyword sets for urgency detection (EN + AR)
    private static readonly HashSet<string> CriticalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "poison", "bleeding", "blood", "choking", "seizure", "collapse", "unconscious",
        "not breathing", "hit by car", "rat poison", "swallowed",
        // Arabic equivalents
        "\u0633\u0645", "\u0646\u0632\u064a\u0641", "\u062f\u0645", "\u0627\u062e\u062a\u0646\u0627\u0642", "\u062a\u0634\u0646\u062c", "\u0627\u0646\u0647\u064a\u0627\u0631", "\u0641\u0627\u0642\u062f \u0627\u0644\u0648\u0639\u064a"
    };

    private static readonly HashSet<string> HighKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "not eating", "lethargic", "vomiting", "diarrhea", "limping", "pain", "swollen",
        "sick", "help", "urgent", "emergency",
        "\u0645\u0631\u064a\u0636", "\u0644\u0627 \u064a\u0623\u0643\u0644", "\u062a\u0642\u064a\u0624", "\u0623\u0644\u0645"
    };

    private static readonly HashSet<string> AppointmentKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "appointment", "reschedule", "cancel", "book", "schedule",
        "\u0645\u0648\u0639\u062f", "\u062d\u062c\u0632"
    };

    private static readonly HashSet<string> AdminKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate", "invoice", "receipt", "vaccination record", "copy",
        "\u0634\u0647\u0627\u062f\u0629", "\u0641\u0627\u062a\u0648\u0631\u0629"
    };

    public Task<MessageClassificationResult?> ClassifyAsync(
        string messageText,
        string? conversationSubject,
        CancellationToken cancellationToken = default)
    {
        var text = $"{conversationSubject} {messageText}".ToLowerInvariant();

        // Short messages → High + Unspecified
        if (messageText.Split(' ').Length <= 2)
            return Task.FromResult<MessageClassificationResult?>(
                new(ClassifiedUrgency.High, ClassifiedCategory.Unspecified, 0.5));

        // Critical keywords
        if (CriticalKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult<MessageClassificationResult?>(
                new(ClassifiedUrgency.Critical, ClassifiedCategory.MedicalConcern, 0.6));

        // ... etc. pour chaque niveau

        // Default: Normal + Other
        return Task.FromResult<MessageClassificationResult?>(
            new(ClassifiedUrgency.Normal, ClassifiedCategory.Other, 0.4));
    }
}
```

### 3. ResilientMessageClassifier (Infrastructure/)

Wrapper avec circuit breaker (pattern identique a ResilientSoapNotesGenerator) :

```csharp
internal sealed class ResilientMessageClassifier : IMessageClassifier
{
    private readonly ClaudeMessageClassifier _primary;
    private readonly KeywordFallbackClassifier _fallback;
    private readonly ILogger<ResilientMessageClassifier> _logger;

    // Circuit breaker state
    private int _consecutiveFailures;
    private DateTime? _circuitOpenUntil;
    private const int FailureThreshold = 3;
    private static readonly TimeSpan CircuitOpenDuration = TimeSpan.FromMinutes(5);

    public async Task<MessageClassificationResult?> ClassifyAsync(...)
    {
        if (IsCircuitOpen())
            return await _fallback.ClassifyAsync(...);

        var result = await _primary.ClassifyAsync(...);
        if (result is not null)
        {
            ResetCircuit();
            return result;
        }

        IncrementFailure();
        return await _fallback.ClassifyAsync(...);
    }
}
```

### 4. DI Registration (MessagingModuleServiceRegistrar.cs)

```csharp
services.AddSingleton<KeywordFallbackClassifier>();
services.AddSingleton<ClaudeMessageClassifier>();
services.AddSingleton<IMessageClassifier, ResilientMessageClassifier>();
```

## Criteres

- [ ] `ClaudeMessageClassifier` appelle l'API Claude avec un prompt structure
- [ ] `KeywordFallbackClassifier` supporte les mots-cles en anglais ET arabe
- [ ] `ResilientMessageClassifier` bascule sur fallback apres 3 echecs consecutifs
- [ ] Le circuit breaker se referme apres 5 minutes
- [ ] Les messages courts (1-2 mots) retournent High + Unspecified (scenario "Help")
- [ ] `dotnet build` passe
- [ ] Tous les classifiers retournent `Result`-compatible (null = fallback, jamais d'exception)
