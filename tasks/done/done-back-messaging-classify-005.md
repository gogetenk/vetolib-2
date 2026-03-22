# todo-back-messaging-classify-005 -- Tests: TU + TI + TF pour message classification

**Module** : Messaging
**Priorite** : critique
**Skills** : `reqnroll-bindings`, `ardalis-result`
**Feature** : `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageClassification.feature`
**Branche** : `feat/messaging-classification-tests`
**Depend de** : todo-back-messaging-classify-001, 002, 003, 004

## Contexte

Le .feature existe deja. Il faut ecrire les step definitions Reqnroll, les TU pour le domain et les classifiers, et les TI pour les endpoints.

## Travail demande

### 1. TU — Tests unitaires (Vetolib.Tests.Unit/Messaging/)

#### Message domain tests

```
MessageClassificationTests.cs
```

- `ApplyClassification_ValidInput_SetsProperties`
- `ApplyClassification_InvalidConfidence_ReturnsError`
- `OverrideClassification_PreservesOriginalAiValues`
- `OverrideClassification_SecondOverride_KeepsFirstOriginalValues`
- `OverrideClassification_NotClassified_ReturnsError`
- `RecordClassificationFeedback_SetsCorrectFlag`
- `RecordClassificationFeedback_NotClassified_ReturnsError`

#### KeywordFallbackClassifier tests

```
KeywordFallbackClassifierTests.cs
```

- `Classify_CriticalKeyword_ReturnsCritical` (test "poison", "bleeding", etc.)
- `Classify_HighKeyword_ReturnsHigh` (test "not eating", "lethargic")
- `Classify_AppointmentKeyword_ReturnsAppointmentRequest`
- `Classify_AdminKeyword_ReturnsAdministrativeRequest`
- `Classify_ShortMessage_ReturnsHighUnspecified` (test "Help")
- `Classify_ArabicCriticalKeyword_ReturnsCritical`
- `Classify_NoKeywords_ReturnsNormalOther`

#### ResilientMessageClassifier tests

```
ResilientMessageClassifierTests.cs
```

- `Classify_PrimarySucceeds_ReturnsPrimaryResult`
- `Classify_PrimaryFails_ReturnsFallbackResult`
- `Classify_ThreeConsecutiveFailures_OpensCircuit`
- `Classify_CircuitOpen_UsesFallbackDirectly`
- `Classify_CircuitRecoversAfterTimeout`

### 2. TI — Tests d'integration (Vetolib.Tests.Integration/Messaging/)

```
MessageClassificationEndpointTests.cs
```

- `OverrideClassification_ValidRequest_Returns200`
- `OverrideClassification_NonExistentMessage_Returns404`
- `OverrideClassification_NotVetRole_Returns403`
- `RecordFeedback_ValidRequest_Returns200`
- `GetClassificationAccuracy_Returns200WithStats`

### 3. TF — Step definitions Reqnroll (Vetolib.Tests.Acceptance/StepDefinitions/Messaging/)

```
MessageClassificationStepDefinitions.cs
```

Implementer les steps pour tous les 12 scenarios du .feature :
- "When pet owner {name} sends a message {text}" — cree une conversation + message via handler
- "Then the message should be automatically classified" — verifie que ClassifiedUrgency/Category sont non-null
- "And the urgency should be {urgency}" — verifie la valeur
- "And the category should be {category}" — verifie la valeur (mapper les strings du Gherkin vers les enums)
- "Then the message should be classified with urgency {urgency}" — idem
- "And the message should be flagged for immediate veterinarian attention" — verifie IsFlaggedForReview ou event publie
- "And all on-duty veterinarians should receive a notification within 1 minute" — verifie que UrgentMessageClassifiedEvent est publie
- "When the veterinarian changes the classification to urgency {urgency} and category {category}" — appelle OverrideMessageClassificationHandler
- "Then the original AI classification should be preserved for training purposes" — verifie OriginalAiUrgency/Category
- "When the staff member confirms the classification is correct" — appelle RecordClassificationFeedbackHandler
- "When I view the classification accuracy report" — appelle GetClassificationAccuracyHandler
- "Then the accuracy rate should show {rate}%" — verifie le DTO

Pour les TF, utiliser le `KeywordFallbackClassifier` comme implementation (pas le vrai Claude API) via DI override dans le test setup. Les TF testent le comportement fonctionnel, pas l'integration AI.

### Mapping Gherkin → enums

| Gherkin value | Enum value |
|---|---|
| "Low" | ClassifiedUrgency.Low |
| "Normal" | ClassifiedUrgency.Normal |
| "High" | ClassifiedUrgency.High |
| "Critical" | ClassifiedUrgency.Critical |
| "Medical concern" | ClassifiedCategory.MedicalConcern |
| "Administrative request" | ClassifiedCategory.AdministrativeRequest |
| "Appointment request" | ClassifiedCategory.AppointmentRequest |
| "Post-operative follow-up" | ClassifiedCategory.PostOperativeFollowUp |
| "Unspecified" | ClassifiedCategory.Unspecified |

## Criteres

- [ ] Tous les 12 scenarios du .feature ont des step definitions
- [ ] Les TU couvrent les edge cases domain (confidence invalide, double override, not classified)
- [ ] Les TU couvrent le fallback classifier (keywords EN + AR, messages courts)
- [ ] Les TU couvrent le circuit breaker (3 echecs, recovery)
- [ ] Les TI couvrent les 3 nouveaux endpoints (override, feedback, accuracy)
- [ ] Les TF utilisent KeywordFallbackClassifier (pas Claude API) via DI
- [ ] `dotnet test` passe sur les 3 projets de tests
- [ ] Zero technique dans les .feature (deja ecrit, ne pas modifier)
