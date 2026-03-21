# AI Features Spec -- Vetolib

**Date** : 2026-03-09
**Auteur** : Architecte Vetolib
**Statut** : Draft -- en attente validation PO + architecte

---

## Table des matieres

1. [Vue d'ensemble](#1-vue-densemble)
2. [Decisions d'architecture transversales](#2-decisions-darchitecture-transversales)
3. [Feature 1 -- AI Triage veterinaire](#3-feature-1--ai-triage-veterinaire)
4. [Feature 2 -- Scheduling optimization](#4-feature-2--scheduling-optimization)
5. [Feature 3 -- Prediction no-show](#5-feature-3--prediction-no-show)
6. [Feature 4 -- AI Messaging / Triage messagerie](#6-feature-4--ai-messaging--triage-messagerie)
7. [Plan d'integration Aspire](#7-plan-dintegration-aspire)
8. [Matrice des risques](#8-matrice-des-risques)

---

## 1. Vue d'ensemble

Quatre features AI a integrer dans Vetolib, reparties en deux categories techniques :

| Feature | Type AI | Lib principale | Module .NET |
|---|---|---|---|
| AI Triage veterinaire | LLM (prompt structured) | Microsoft.Extensions.AI | Nouveau : `Vetolib.AI` |
| Scheduling optimization | Rule-based + heuristique | Code .NET pur (aucune lib AI) | `Vetolib.Agenda` (extension) |
| Prediction no-show | ML classique (regression logistique) | ML.NET | Nouveau : `Vetolib.AI` |
| AI Messaging / Triage messagerie | LLM (classification + generation) | Microsoft.Extensions.AI | Nouveau : `Vetolib.AI` |

---

## 2. Decisions d'architecture transversales

### 2.1 Nouveau module `Vetolib.AI`

Les features 1, 3 et 4 necessitent un nouveau module AI. La feature 2 (scheduling) est purement algorithmique et s'integre dans le module Agenda existant.

```
Modules/
  AI/
    Vetolib.AI.Contracts/          <-- PUBLIC : DTOs, enums, interfaces
      TriageSuggestionDto.cs
      NoShowPredictionDto.cs
      MessageTriageDto.cs
      AISeverity.cs                <-- enum: Emergency, Normal, Routine
      MessageCategory.cs           <-- enum: Emergency, AdminQuestion, AppointmentRequest, PostOpFollowUp
      IAITriageService.cs          <-- interface pour injection cross-module
      Vetolib.AI.Contracts.csproj
    Vetolib.AI/                    <-- INTERNAL : handlers, services, modeles ML.NET
      Application/
        Commands/
          TriageSymptoms/
            TriageSymptomsCommand.cs
            TriageSymptomsHandler.cs
            TriageSymptomsValidator.cs
          TriageMessage/
            TriageMessageCommand.cs
            TriageMessageHandler.cs
          PredictNoShow/
            PredictNoShowCommand.cs
            PredictNoShowHandler.cs
        Queries/
          GetNoShowHistory/
            GetNoShowHistoryQuery.cs
            GetNoShowHistoryHandler.cs
        Services/
          VetTriageLlmService.cs       <-- encapsule IChatClient
          MessageTriageLlmService.cs   <-- encapsule IChatClient
          NoShowPredictionService.cs   <-- encapsule ML.NET PredictionEngine
        Domain/
          TriageResult.cs              <-- entity persistee pour audit
          MessageTriage.cs             <-- entity persistee pour audit
        ML/
          NoShowModel.mbconfig         <-- modele ML.NET serialise
          NoShowInput.cs               <-- schema d'entree ML.NET
          NoShowOutput.cs              <-- schema de sortie ML.NET
      Infrastructure/
        AIDbContext.cs
        TriageResultConfiguration.cs
      ModuleServiceRegistrar.cs
      Vetolib.AI.csproj
```

**Justification du module separe** : les features AI n'ont pas de couplage fort entre elles, mais partagent l'infrastructure LLM (`IChatClient`) et la configuration du provider AI. Un module unique evite de disperser la configuration AI dans chaque module metier. Le module AI consomme les Contracts des autres modules (Agenda, MedicalRecords) sans jamais referencer leur runtime -- conforme a la regle d'isolation.

### 2.2 Abstraction AI : `Microsoft.Extensions.AI`

`Microsoft.Extensions.AI` (package `Microsoft.Extensions.AI.Abstractions` + `Microsoft.Extensions.AI.OpenAI`) fournit :
- `IChatClient` -- abstraction unifiee pour tout LLM (OpenAI, Ollama, Azure OpenAI)
- `IEmbeddingGenerator<T>` -- si RAG necessaire plus tard
- Integration native OpenTelemetry (traces des appels LLM visibles dans Aspire Dashboard)
- Middleware pipeline (logging, caching, rate limiting des appels LLM)

```csharp
// Registration dans ModuleServiceRegistrar.cs
public static IServiceCollection AddAIModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // IChatClient pipeline avec telemetrie + caching
    services.AddChatClient(builder =>
        builder
            .UseOpenTelemetry()        // traces automatiques dans Aspire
            .UseFunctionInvocation()   // tool calling si besoin
            .UseDistributedCache()     // cache des reponses identiques
            .Use(innerClient));        // provider concret (OpenAI ou Ollama)

    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly));

    services.AddSingleton<NoShowPredictionService>();

    return services;
}
```

### 2.3 Provider AI : dual-mode (dev local / production)

| Environnement | Provider | Configuration Aspire |
|---|---|---|
| Dev local | Ollama (llama3.1 8B ou mistral 7B) | `Aspire.Hosting.Ollama` |
| Production | Azure OpenAI (GPT-4o) | `Aspire.Hosting.Azure.CognitiveServices` |

```csharp
// AppHost/Program.cs -- ajout AI
var ollama = builder.AddOllama("ollama")
    .AddModel("llama3.1")
    .WithDataVolume();

// OU en production :
// var openai = builder.AddAzureOpenAI("openai")
//     .AddDeployment(new("gpt-4o", "gpt-4o", "2024-08-06"));

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WithReference(rabbitmq)
    .WithReference(ollama)          // injecte la connection string Ollama
    // ...
```

### 2.4 Pattern Result<T> -- applique a toutes les features AI

Chaque handler AI retourne `Result<T>`. Les erreurs possibles :
- `Result.Error("AI_SERVICE_UNAVAILABLE")` -- provider LLM indisponible
- `Result.Error("AI_RESPONSE_INVALID")` -- reponse LLM non parsable
- `Result.Error("ML_MODEL_NOT_LOADED")` -- modele ML.NET pas encore entraine
- `Result.Invalid(...)` -- validation FluentValidation en amont

### 2.5 Telemetrie AI dans Aspire

`Microsoft.Extensions.AI` instrumente automatiquement les appels LLM via OpenTelemetry :
- **Traces** : chaque appel `IChatClient.CompleteAsync()` cree un span avec model, tokens in/out, latence
- **Metriques** : compteurs de tokens consommes, latence p50/p95/p99, taux d'erreur
- **Logs** : prompt envoye et reponse recue (masquables en production via Serilog sensitive enricher)

Le `ServiceDefaults/Extensions.cs` existant n'a pas besoin de modification -- le pipeline OpenTelemetry existant capte automatiquement les Activity emises par `Microsoft.Extensions.AI`.

Custom metrics a ajouter dans le module AI :

```csharp
// Compteur metier visible dans Aspire Dashboard
private static readonly Meter AIMeter = new("Vetolib.AI", "1.0");
private static readonly Counter<int> TriageCounter =
    AIMeter.CreateCounter<int>("vetolib.ai.triage.count", "Count of triage requests");
private static readonly Histogram<double> TriageLatency =
    AIMeter.CreateHistogram<double>("vetolib.ai.triage.duration_ms", "Triage latency in ms");
```

Enregistrement dans `ServiceDefaults/Extensions.cs` (ajout non-breaking) :

```csharp
metrics.AddMeter("Vetolib.AI");
```

---

## 3. Feature 1 -- AI Triage veterinaire

### 3.1 Description fonctionnelle

Lors de la prise de RDV (par le proprietaire ou la receptionniste), les symptomes decrits en texte libre sont analyses par un LLM pour produire :
- **Severite suggeree** : `Emergency` | `Normal` | `Routine`
- **Duree estimee** de consultation (en minutes)
- **Specialite recommandee** : generaliste, chirurgie, dermatologie, dentaire, ophtalmologie, urgences
- **Disclaimer legal** : texte fixe, toujours present, non genere par le LLM

Le veterinaire ou la receptionniste **valide ou corrige** la suggestion avant confirmation du RDV. L'IA ne prend jamais de decision autonome.

### 3.2 Architecture technique

```
Composant frontend                 Module AI                        LLM Provider
+-----------------------+     +------------------------+     +------------------+
| Formulaire prise RDV  |     | TriageSymptomsHandler  |     | Ollama (dev)     |
| - champ symptomes     |---->| - valide input         |---->| Azure OpenAI     |
| - affiche suggestion  |     | - construit prompt     |     | (prod)           |
| - disclaimer legal    |<----| - parse reponse JSON   |<----|                  |
| - vet valide/corrige  |     | - retourne Result<T>   |     +------------------+
+-----------------------+     | - persiste pour audit  |
                              +------------------------+
                                       |
                                       v
                              +------------------+
                              | AIDbContext       |
                              | triage_results    |
                              +------------------+
```

### 3.3 Contrat API

**Endpoint** : `POST /api/ai/triage`
**Auth** : requis (roles: Vet, Receptionist, Admin)
**Rate limit** : policy "api" (100/min)

Request :
```json
{
  "symptoms": "Mon chat vomit depuis 2 jours et refuse de manger",
  "species": "cat",
  "breed": "persian",
  "ageMonths": 36,
  "weightKg": 4.2
}
```

Response (200) :
```json
{
  "severity": "Normal",
  "estimatedDurationMinutes": 30,
  "recommendedSpecialty": "generaliste",
  "reasoning": "Vomissements de 2 jours avec anorexie chez un chat adulte. Necessite examen clinique et potentiellement bilan sanguin. Pas de signe d'urgence vitale immediate.",
  "disclaimer": "Cette suggestion est generee par une intelligence artificielle a titre indicatif uniquement. Elle ne constitue pas un diagnostic veterinaire. Seul un veterinaire diplome peut poser un diagnostic et prescrire un traitement. En cas de doute sur la gravite, contactez immediatement votre veterinaire.",
  "confidence": 0.82,
  "triageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### 3.4 Handler MediatR

```csharp
internal record TriageSymptomsCommand(
    string Symptoms,
    string Species,
    string? Breed,
    int? AgeMonths,
    decimal? WeightKg) : IRequest<Result<TriageSuggestionDto>>;

internal class TriageSymptomsHandler : IRequestHandler<TriageSymptomsCommand, Result<TriageSuggestionDto>>
{
    private readonly IChatClient _chatClient;
    private readonly AIDbContext _context;
    private readonly IClinicContext _clinicContext;

    // Le handler :
    // 1. Valide les inputs (FluentValidation via pipeline behavior)
    // 2. Construit un system prompt structure (JSON mode)
    // 3. Appelle IChatClient.CompleteAsync()
    // 4. Parse la reponse JSON
    // 5. Persiste le TriageResult dans AIDbContext (audit trail)
    // 6. Retourne Result<TriageSuggestionDto> avec disclaimer constant
}
```

### 3.5 Prompt engineering

Le system prompt impose :
- Reponse en JSON strict (schema fourni)
- Role : assistant de triage veterinaire, jamais de diagnostic
- Langue : francais
- Interdiction de recommander des medicaments
- Classification en 3 niveaux uniquement (Emergency / Normal / Routine)
- Justification obligatoire en 1-2 phrases

Temperature : 0.1 (reponses deterministes).

### 3.6 Modele recommande

| Environnement | Modele | Justification |
|---|---|---|
| Dev local | llama3.1:8b (Ollama) | Gratuit, tourne sur GPU 8GB, JSON mode supporte |
| Production | GPT-4o (Azure OpenAI) | Meilleure comprehension medicale, JSON mode natif, SLA |

### 3.7 Persistance (audit)

Chaque triage est persiste dans la table `ai.triage_results` (schema `ai`, multi-tenant) :
- `Id`, `ClinicId`, `Symptoms`, `Species`, `Breed`, `AgeMonths`, `WeightKg`
- `SuggestedSeverity`, `EstimatedDuration`, `RecommendedSpecialty`, `Reasoning`, `Confidence`
- `WasAccepted` (bool -- le vet a-t-il accepte la suggestion ?)
- `OverriddenSeverity` (si le vet a corrige)
- `ModelUsed` (ex: "gpt-4o-2024-08-06")
- `PromptTokens`, `CompletionTokens`, `LatencyMs`
- `CreatedAt`, `CreatedBy`

Ce log permet :
- Audit reglementaire (tracabilite des suggestions AI)
- Fine-tuning futur (paires input/correction humaine)
- Metriques de qualite (taux d'acceptation des suggestions)

### 3.8 Considerations ethiques/legales

- Disclaimer legal **constant, non genere par le LLM**, affiche systematiquement cote client ET retourne dans l'API
- Le champ `WasAccepted` prouve que le veterinaire a valide/corrige
- Aucun diagnostic n'est formule -- uniquement une classification de severite
- Les donnees de triage sont liees au tenant (multi-tenancy) et ne sont jamais partagees entre cliniques
- RGPD : les symptomes sont des donnees de sante animale (pas humaine), mais le nom du proprietaire est une donnee personnelle -- respecter le droit a l'effacement

### 3.9 Estimation de complexite

**Taille : M (Medium)**

- Nouveau module a scaffolder (M)
- Integration `Microsoft.Extensions.AI` + configuration dual-mode (S)
- Prompt engineering + parsing JSON (S)
- Endpoint + handler + validator + persistance (S)
- Frontend : ajout champ symptomes + affichage suggestion dans le formulaire RDV (M)
- Tests : BDD scenarios (triage accepte, triage corrige, LLM indisponible) (S)

---

## 4. Feature 2 -- Scheduling optimization

### 4.1 Description fonctionnelle

Lors de la prise de RDV, le systeme suggere le meilleur creneau en fonction de :
- **Type de consultation** : duree attendue
- **Historique des durees** par veterinaire (durees reelles vs. prevues)
- **Creneaux disponibles** : minimiser les trous dans le planning
- **Equilibrage de charge** : repartir equitablement entre veterinaires
- **Regroupement par type** : essayer de grouper les consultations similaires (chirurgies le matin, suivis l'apres-midi)

### 4.2 Architecture technique

**Pas de LLM, pas de ML.NET.** Cette feature est purement algorithmique (rule-based + heuristique de scoring).

```
Module Agenda (extension)
+----------------------------------+
| Application/                     |
|   Queries/                       |
|     SuggestSlot/                 |
|       SuggestSlotQuery.cs        |
|       SuggestSlotHandler.cs      |
|   Services/                      |
|     SlotScoringService.cs        |  <-- algorithme de scoring
|     DurationEstimator.cs         |  <-- moyenne glissante des durees reelles
+----------------------------------+
         |
         v
+----------------------------------+
| AgendaDbContext                  |
| - Appointments (historique)      |
| - VetSchedules (config)         |
+----------------------------------+
```

### 4.3 Algorithme de scoring

Chaque creneau disponible recoit un score (0-100) base sur :

| Critere | Poids | Calcul |
|---|---|---|
| Minimise les gaps | 30% | Penalite proportionnelle au trou (minutes) entre ce creneau et le RDV precedent/suivant |
| Equilibrage charge | 25% | Bonus si le vet a moins de RDV ce jour-la que la moyenne des vets |
| Regroupement type | 20% | Bonus si le vet a deja des consultations du meme type ce jour-la |
| Preference horaire vet | 15% | Bonus si le creneau est dans la plage preferee du vet |
| Proximite demande client | 10% | Bonus si le creneau est proche de l'heure demandee par le client |

Le handler retourne les 3 meilleurs creneaux tries par score decroissant.

### 4.4 Estimation de duree

`DurationEstimator` calcule la duree estimee via une moyenne glissante des 20 derniers RDV du meme type pour le meme vet. Fallback sur la duree par defaut du type de consultation si pas assez d'historique.

```csharp
internal class DurationEstimator
{
    // Moyenne glissante des 20 dernieres consultations du vet pour ce type
    // Retourne Result<int> (minutes)
    // Si < 5 RDV historiques : retourne la duree par defaut du type
    public async Task<Result<int>> EstimateAsync(
        Guid veterinarianId,
        string consultationType,
        CancellationToken ct);
}
```

### 4.5 Contrat API

**Endpoint** : `POST /api/agenda/suggest-slot`
**Auth** : requis (roles: Vet, Receptionist, Admin)

Request :
```json
{
  "consultationType": "general",
  "preferredDate": "2026-03-15",
  "preferredTime": "10:00",
  "preferredVeterinarianId": null,
  "durationMinutes": null
}
```

Response (200) :
```json
{
  "suggestions": [
    {
      "veterinarianId": "...",
      "veterinarianName": "Dr. Ahmad",
      "date": "2026-03-15",
      "startTime": "10:15",
      "estimatedDurationMinutes": 25,
      "score": 87,
      "reasoning": "Creneau adjacent au RDV precedent (pas de gap), charge equilibree"
    },
    {
      "veterinarianId": "...",
      "veterinarianName": "Dr. Fatima",
      "date": "2026-03-15",
      "startTime": "10:00",
      "estimatedDurationMinutes": 25,
      "score": 72,
      "reasoning": "Heure exacte demandee, mais cree un gap de 30 min dans le planning"
    }
  ]
}
```

### 4.6 Integration dans le module Agenda existant

Pas de nouveau module. Ajouts dans `Vetolib.Agenda` :
- `Application/Queries/SuggestSlot/SuggestSlotQuery.cs`
- `Application/Queries/SuggestSlot/SuggestSlotHandler.cs`
- `Application/Services/SlotScoringService.cs`
- `Application/Services/DurationEstimator.cs`

Ajouts dans `Vetolib.Agenda.Contracts` :
- `SlotSuggestionDto.cs`
- `SuggestSlotRequest.cs`

Ajout dans les endpoints Agenda (endpoint mapping dans `AppointmentEndpoints.cs`).

### 4.7 Considerations

- Pas de problematique ethique (pas d'IA, pas de donnees sensibles)
- Algorithme entierement deterministe et testable unitairement
- Les poids du scoring sont configurables via `appsettings.json` section `Agenda:SlotScoring`

### 4.8 Estimation de complexite

**Taille : M (Medium)**

- `SlotScoringService` avec 5 criteres ponderes (M)
- `DurationEstimator` avec requete historique (S)
- Query handler + endpoint (S)
- Tests unitaires (scoring edge cases, pas assez d'historique, journee pleine) (M)
- Frontend : proposer les creneaux dans le formulaire de prise de RDV (S)

---

## 5. Feature 3 -- Prediction no-show

### 5.1 Description fonctionnelle

Pour chaque RDV planifie, le systeme calcule un score de probabilite de no-show (0-100%). Si le score depasse 30%, il suggere de :
- Envoyer un rappel supplementaire (SMS/email)
- Activer l'overbooking sur ce creneau (proposer le creneau a un deuxieme patient en liste d'attente)

### 5.2 Architecture technique

```
Module AI                             ML.NET
+-------------------------------+    +---------------------------+
| PredictNoShowHandler          |    | Modele entraine           |
| - collecte features           |--->| LogisticRegression        |
| - appelle PredictionEngine    |    | Features:                 |
| - retourne Result<T>         |<---| - historical_noshow_rate  |
+-------------------------------+    | - day_of_week             |
         |                           | - hour_of_day             |
         v                           | - days_since_last_visit   |
+-------------------------------+    | - appointment_type        |
| Agenda.Contracts              |    | - owner_appointment_count |
| (lecture historique via       |    +---------------------------+
|  interface IAppointmentReader)|
+-------------------------------+
```

### 5.3 Features du modele ML.NET

| Feature | Type | Source |
|---|---|---|
| `historical_noshow_rate` | float | Taux de no-show du owner sur ses 20 derniers RDV |
| `day_of_week` | int (0-6) | Jour de la semaine du RDV |
| `hour_of_day` | float | Heure du RDV (ex: 14.5 pour 14h30) |
| `days_since_last_visit` | int | Jours depuis le dernier RDV complete du owner |
| `appointment_type` | string (one-hot) | Type de consultation (general, chirurgie, suivi...) |
| `owner_total_appointments` | int | Nombre total de RDV historiques du owner |
| `was_reminder_sent` | bool | Un rappel a-t-il ete envoye |
| `lead_time_days` | int | Jours entre la prise de RDV et la date du RDV |

### 5.4 Entrainement du modele

ML.NET `AutoML` ou `BinaryClassification.Trainers.SdcaLogisticRegression`.

**Donnees d'entrainement** : extraites des appointments historiques (statut `Completed` vs. `NoShow`).

**Pipeline d'entrainement** (batch, offline) :

```csharp
var pipeline = mlContext.Transforms.Categorical
    .OneHotEncoding("appointment_type_encoded", "appointment_type")
    .Append(mlContext.Transforms.Concatenate("Features",
        "historical_noshow_rate", "day_of_week", "hour_of_day",
        "days_since_last_visit", "owner_total_appointments",
        "was_reminder_sent", "lead_time_days",
        "appointment_type_encoded"))
    .Append(mlContext.BinaryClassification.Trainers
        .SdcaLogisticRegression(labelColumnName: "is_noshow"));
```

Le modele entraine est serialise en `.zip` et charge au demarrage via `PredictionEngine<NoShowInput, NoShowOutput>` (thread-safe via `PredictionEnginePool`).

**Reentrainement** : job planifie (MassTransit scheduler ou Hangfire) qui reentraince le modele chaque semaine avec les nouvelles donnees.

### 5.5 Contrat API

**Endpoint** : `GET /api/ai/no-show-prediction/{appointmentId}`
**Auth** : requis (roles: Vet, Receptionist, Admin)

Response (200) :
```json
{
  "appointmentId": "...",
  "noShowProbability": 0.42,
  "riskLevel": "High",
  "topFactors": [
    "Proprietaire a 35% de no-show historique",
    "RDV le lundi matin (taux de no-show +15% pour cette clinique)",
    "Prise de RDV il y a plus de 14 jours"
  ],
  "suggestions": [
    "Envoyer un rappel SMS supplementaire 2h avant",
    "Envisager un overbooking sur ce creneau"
  ]
}
```

**Endpoint batch** : `POST /api/ai/no-show-predictions/batch`
Pour le dashboard : prediction pour tous les RDV du jour.

Request :
```json
{
  "date": "2026-03-15"
}
```

### 5.6 Communication inter-modules

Le module AI a besoin de lire l'historique des RDV (module Agenda) et les donnees owner (module MedicalRecords). Conformement a la regle d'isolation :

**Option retenue : interface dans Contracts + implementation dans le module source**

```csharp
// Vetolib.Agenda.Contracts/IAppointmentReader.cs
public interface IAppointmentReader
{
    Task<IReadOnlyList<AppointmentHistoryDto>> GetOwnerHistoryAsync(
        Guid ownerId, int limit, CancellationToken ct);
}

// Vetolib.Agenda/Application/Services/AppointmentReader.cs (internal)
internal class AppointmentReader : IAppointmentReader { ... }
```

Le module AI reference `Vetolib.Agenda.Contracts` (pas le runtime) et consomme `IAppointmentReader` via DI.

### 5.7 Gestion du cold start

Quand une clinique n'a pas assez d'historique (< 50 RDV avec des no-shows marques), le modele retourne `Result.Error("INSUFFICIENT_DATA")` avec un message explicatif. Pas de prediction hasardeuse.

### 5.8 Estimation de complexite

**Taille : L (Large)**

- Infrastructure ML.NET + PredictionEnginePool (M)
- Feature engineering (collecte depuis Agenda + MedicalRecords) (M)
- Interfaces inter-modules dans Contracts (S)
- Pipeline d'entrainement + reentrainement periodique (L)
- Endpoint prediction unitaire + batch (S)
- Frontend : indicateur visuel no-show risk dans le planning (S)
- Tests : modele avec donnees synthetiques, cold start, predictions limites (M)

---

## 6. Feature 4 -- AI Messaging / Triage messagerie

### 6.1 Description fonctionnelle

Les proprietaires envoient des messages texte a la clinique. L'IA :
1. **Categorise** le message : `Emergency` | `AdminQuestion` | `AppointmentRequest` | `PostOpFollowUp` | `Other`
2. **Suggere une reponse** pre-redigee au praticien (qui valide/modifie avant envoi)
3. **Resume** l'historique de conversation pour le veterinaire
4. Publie un **evenement d'integration** vers le module Notifications si categorie `Emergency`

### 6.2 Architecture technique

```
Proprietaire          Module AI                      Module Notifications
+----------+    +---------------------------+    +------------------------+
| Message  |--->| TriageMessageHandler      |    | Consumer:              |
| (texte)  |    | - classification LLM      |    | EmergencyMessageEvent  |
+----------+    | - generation reponse      |    | -> alerte SMS/email    |
                | - persistance             |    | au vet de garde        |
                | - publie event si urgence |    +------------------------+
                +---------------------------+
                         |
                         v
                +---------------------------+
                | SummarizeConversation     |
                | Handler                   |
                | - resume N derniers msgs  |
                | - retourne texte concis   |
                +---------------------------+
```

### 6.3 Entite domaine : Message et Conversation

Nouveau domaine dans le module AI (ou dans un futur module Messaging) :

```csharp
internal class Conversation : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string OwnerName { get; private set; }
    public Guid? PatientId { get; private set; }        // animal concerne (optionnel)
    public ConversationStatus Status { get; private set; } // Open, Resolved, Escalated
    public List<Message> Messages { get; private set; }
}

internal class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public MessageSender Sender { get; private set; }   // Owner, AI, Vet
    public string Content { get; private set; }
    public MessageCategory? AiCategory { get; private set; }
    public string? AiSuggestedReply { get; private set; }
    public bool WasSuggestedReplyUsed { get; private set; }
    public string? ActualReply { get; private set; }    // ce que le vet a reellement envoye
}
```

### 6.4 Contrats API

**Endpoint 1** : `POST /api/ai/messages/triage`
Categorise un message entrant et suggere une reponse.

Request :
```json
{
  "conversationId": "...",
  "content": "Mon chien a mange du chocolat il y a 1 heure, il tremble",
  "ownerId": "...",
  "patientId": "..."
}
```

Response (200) :
```json
{
  "messageId": "...",
  "category": "Emergency",
  "confidence": 0.95,
  "suggestedReply": "Bonjour, l'ingestion de chocolat est potentiellement dangereuse pour votre chien. Veuillez vous rendre immediatement aux urgences veterinaires. En attendant, ne faites pas vomir votre animal sans avis veterinaire. Etes-vous en mesure de vous deplacer maintenant ?",
  "disclaimer": "Cette reponse a ete pre-redigee par une intelligence artificielle. Elle sera relue et validee par un veterinaire avant envoi.",
  "isEmergency": true
}
```

**Endpoint 2** : `POST /api/ai/messages/{messageId}/send`
Le vet valide (ou modifie) la reponse suggeree et l'envoie.

Request :
```json
{
  "reply": "Bonjour, c'est le Dr. Ahmad. L'ingestion de chocolat est effectivement une urgence...",
  "usedSuggestedReply": false
}
```

**Endpoint 3** : `GET /api/ai/conversations/{conversationId}/summary`
Resume de l'historique de la conversation pour le vet.

Response (200) :
```json
{
  "conversationId": "...",
  "messageCount": 12,
  "summary": "Proprietaire: Mme Al-Rashid. Patient: Luna (chat, 3 ans). Echange sur 3 jours. Sujet initial: vomissements post-operatoires. Le chat a ete opere le 05/03 (ovariectomie). Vomissements signales le 06/03, amelioration le 07/03 apres ajustement alimentation. Dernier message: confirmation que Luna mange normalement.",
  "timeline": [
    { "date": "2026-03-06", "summary": "Signalement vomissements post-op" },
    { "date": "2026-03-07", "summary": "Amelioration apres conseil alimentation" },
    { "date": "2026-03-08", "summary": "Confirmation retour a la normale" }
  ]
}
```

### 6.5 Integration avec le module Notifications (MassTransit)

Quand la categorie est `Emergency`, le handler publie un evenement d'integration :

```csharp
// Vetolib.AI.Contracts/Events/EmergencyMessageReceivedEvent.cs
public record EmergencyMessageReceivedEvent(
    Guid ClinicId,
    Guid ConversationId,
    Guid OwnerId,
    string OwnerName,
    string MessageContent,
    string AiCategory,
    DateTime ReceivedAt);
```

Le module Notifications consomme cet evenement et envoie une alerte SMS/email au veterinaire de garde. Le consumer est dans `Vetolib.Notifications` -- conforme a l'isolation (le module AI ne reference que `Vetolib.Notifications.Contracts` pour l'evenement).

### 6.6 Prompt engineering

Deux prompts distincts :

**Prompt classification** (temperature 0.0) :
- Input : message du proprietaire + contexte (espece, age, historique recent)
- Output : JSON `{ "category": "...", "confidence": 0.0-1.0, "reasoning": "..." }`
- Contrainte : classification parmi les 5 categories uniquement

**Prompt generation de reponse** (temperature 0.3) :
- Input : message + categorie + historique conversation (5 derniers messages)
- Output : texte de reponse en francais, professionnel, empathique
- Contrainte : ne jamais prescrire, ne jamais diagnostiquer, orienter vers la clinique si doute
- Contrainte : tutoiement/vouvoiement configurable par clinique

**Prompt resume** (temperature 0.1) :
- Input : tous les messages de la conversation
- Output : resume structure (contexte, timeline, statut actuel)
- Contrainte : max 200 mots

### 6.7 Considerations ethiques/legales

- **Validation humaine obligatoire** : chaque reponse AI est relue par le vet avant envoi au proprietaire
- Le champ `WasSuggestedReplyUsed` + `ActualReply` tracent la modification humaine
- **Disclaimer** affiche dans le frontend + retourne dans l'API
- **Urgences** : l'IA categorise mais ne decide pas du routing -- le vet de garde est notifie pour decision
- **Confidentialite** : les conversations sont multi-tenant, jamais partagees entre cliniques
- **Retention** : politique de retention des messages a definir avec le PO (ex: 2 ans puis anonymisation)

### 6.8 Estimation de complexite

**Taille : XL (Extra Large)**

- Entites domaine Conversation + Message (M)
- 3 endpoints avec handlers (M)
- 3 prompts a concevoir et tester (M)
- Integration MassTransit pour les urgences (S)
- Frontend : interface de messagerie avec suggestion AI, validation vet, historique (XL)
- Persistance + DbContext (S)
- Tests : classification edge cases, generation reponse, resume, urgence -> notification (L)

---

## 7. Plan d'integration Aspire

### 7.1 Modifications AppHost/Program.cs

```csharp
// -- Ajout AI Provider --
// Dev : Ollama local (GPU optionnel, CPU fallback)
var ollama = builder.AddOllama("ollama")
    .AddModel("llama3.1")
    .WithDataVolume()
    .WithContainerRuntimeArgs("--gpus=all");  // GPU si disponible

// Prod (commenter Ollama, decommmenter Azure OpenAI) :
// var openai = builder.AddAzureOpenAI("openai")
//     .AddDeployment(new("gpt-4o", "gpt-4o", "2024-08-06"));

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WithReference(rabbitmq)
    .WithReference(ollama)           // OU .WithReference(openai)
    // ...
```

### 7.2 Modifications Vetolib.Api/Program.cs

```csharp
// AI module -- nouveau DbContext
builder.AddNpgsqlDbContext<AIDbContext>("vetolibdb");

// AI module registration
builder.Services.AddAIModule(builder.Configuration);

// Audit interceptor pour AI
builder.Services.AddAuditInterceptor<AIDbContext>();

// Endpoint mapping
app.MapAIEndpoints();
```

### 7.3 Nouveaux packages NuGet

| Package | Projet | Usage |
|---|---|---|
| `Microsoft.Extensions.AI.Abstractions` | `Vetolib.AI.Contracts` | `IChatClient` interface |
| `Microsoft.Extensions.AI.OpenAI` | `Vetolib.AI` | Provider OpenAI/Azure OpenAI |
| `Microsoft.Extensions.AI.Ollama` | `Vetolib.AI` | Provider Ollama (dev) |
| `ML.NET` | `Vetolib.AI` | Prediction no-show |
| `Aspire.Hosting.Ollama` | `AppHost` | Hosting Ollama dans Aspire |

### 7.4 Telemetrie visible dans Aspire Dashboard

Apres integration, le dashboard Aspire affichera :

- **Traces** : `POST /api/ai/triage` -> span `chat gpt-4o` (duree, tokens)
- **Metriques** : `vetolib.ai.triage.count`, `vetolib.ai.triage.duration_ms`, `vetolib.ai.noshow.predictions`
- **Logs structurees** : prompt/response (masquees en prod via Serilog sensitive data)
- **Dependance** : le graphe de dependances montrera `api` -> `ollama` (ou `openai`)

---

## 8. Matrice des risques

| Risque | Impact | Probabilite | Mitigation |
|---|---|---|---|
| LLM indisponible (Ollama crash, Azure quota) | Haut | Moyenne | Fallback gracieux : `Result.Error("AI_SERVICE_UNAVAILABLE")`, le formulaire de RDV fonctionne sans triage |
| LLM hallucine une urgence inexistante | Haut | Faible | Temperature basse (0.0-0.1), validation humaine obligatoire, disclaimer legal |
| LLM manque une vraie urgence | Critique | Faible | Le triage est une **suggestion**, pas un filtre. Le proprietaire peut toujours appeler en urgence. Le message d'urgence du proprietaire arrive aussi en brut au vet |
| Modele no-show biaise (discrimine certains owners) | Moyen | Moyenne | Features non-discriminatoires (pas de nom, pas d'adresse), monitoring du taux de faux positifs par segment |
| Cout tokens OpenAI en production | Moyen | Haute | Cache distribue sur les prompts identiques, monitoring cout dans les metriques Aspire, rate limiting par clinique |
| Cold start ML.NET (pas assez de donnees) | Faible | Haute pour nouvelles cliniques | Retourne `Result.Error("INSUFFICIENT_DATA")` explicite, pas de prediction hasardeuse |
| Latence LLM degradant l'UX | Moyen | Moyenne | Appels AI asynchrones (non-bloquants dans le flow de prise de RDV), streaming SSE pour la messagerie |
| RGPD / donnees de sante | Haut | Faible | Les symptomes sont des donnees animales. Seul le nom du proprietaire est une donnee personnelle -- gere par le droit a l'effacement existant |

---

## Annexe : Ordre d'implementation recommande

| Phase | Feature | Pre-requis |
|---|---|---|
| Phase 1 | Scheduling optimization (Feature 2) | Aucun -- purement algorithmique, dans le module Agenda existant |
| Phase 2 | AI Triage veterinaire (Feature 1) | Scaffolding module AI, integration `Microsoft.Extensions.AI` + Aspire Ollama |
| Phase 3 | Prediction no-show (Feature 3) | Module AI existant, interfaces inter-modules dans Contracts, ML.NET |
| Phase 4 | AI Messaging (Feature 4) | Module AI mature, entites Conversation/Message, integration Notifications |

La phase 1 peut demarrer immediatement car elle ne depend d'aucune infrastructure AI. Les phases 2-4 dependent du scaffolding du module AI et de l'integration Aspire Ollama.
