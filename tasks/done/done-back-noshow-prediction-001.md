# todo-back-noshow-prediction-001.md — Prediction no-show ML.NET

**Module** : AI
**Dependances** : done-back-noshow-contracts-001
**Priorite** : MOYENNE (Phase 3)
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Predire la probabilite de no-show pour un RDV donne via un modele ML.NET (regression logistique).

## Spec de reference

`docs/AI-FEATURES-SPEC.md` section 5 (Feature 3 — Prediction no-show)

## Implementation

### ML.NET model

- `NoShowInput.cs` — 8 features : historical_noshow_rate, day_of_week, hour_of_day, days_since_last_visit, appointment_type, owner_total_appointments, was_reminder_sent, lead_time_days
- `NoShowOutput.cs` — Probability, PredictedLabel
- `NoShowPredictionService.cs` — PredictionEnginePool, thread-safe
- Pipeline : OneHotEncoding(appointment_type) → Concatenate → SdcaLogisticRegression

### Handler

- `PredictNoShowCommand` + `PredictNoShowHandler`
- Collecte features via `IAppointmentReader` (Agenda.Contracts)
- Cold start : retourne `Result.Error("INSUFFICIENT_DATA")` si < 50 RDV historiques

### Endpoints

- `GET /api/ai/no-show-prediction/{appointmentId}` — prediction unitaire
- `POST /api/ai/no-show-predictions/batch` — prediction batch pour un jour donne

### Response

```json
{
  "appointmentId": "...",
  "noShowProbability": 0.42,
  "riskLevel": "High",
  "topFactors": ["..."],
  "suggestions": ["..."]
}
```

### Regles metier (PO)

- Le score no-show ne doit JAMAIS etre montre au proprietaire (interne clinique uniquement)
- Features non-discriminatoires (pas de nom, pas d'adresse)

## Critere

```
[] Feature file creee
[] ML.NET model avec 8 features
[] PredictionEnginePool thread-safe
[] Cold start gere (INSUFFICIENT_DATA)
[] Endpoint unitaire + batch
[] Score invisible pour le proprietaire
[] Tests unitaires (donnees synthetiques)
[] Gherkins GREEN
[] Renommer en done
```
