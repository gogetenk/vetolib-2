# todo-back-scheduling-001.md — Scheduling optimization (slot suggestion)

**Module** : Agenda (extension)
**Dependances** : aucune
**Priorite** : HAUTE (Phase 1 AI features — aucune dep AI)
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Suggerer le meilleur creneau de RDV en fonction de l'historique, la charge des vets, et les preferences client. Algorithme purement deterministe (pas de LLM/ML).

## Spec de reference

`docs/AI-FEATURES-SPEC.md` section 4 (Feature 2 — Scheduling optimization)

## Implementation

### Backend (dans Vetolib.Agenda)

1. **Query handler** : `Application/Queries/SuggestSlot/`
   - `SuggestSlotQuery.cs` — record avec `ConsultationType`, `PreferredDate`, `PreferredTime`, `PreferredVeterinarianId?`, `DurationMinutes?`
   - `SuggestSlotHandler.cs` — orchestre scoring + estimation duree

2. **Services** : `Application/Services/`
   - `SlotScoringService.cs` — scoring 0-100 sur 5 criteres ponderes :
     - Minimise gaps (30%)
     - Equilibrage charge (25%)
     - Regroupement type (20%)
     - Preference horaire vet (15%)
     - Proximite demande client (10%)
   - `DurationEstimator.cs` — moyenne glissante des 20 derniers RDV du meme type par vet. Fallback sur duree defaut si < 5 RDV historiques.

3. **Contracts** : `Vetolib.Agenda.Contracts/`
   - `SlotSuggestionDto.cs`
   - `SuggestSlotRequest.cs`

4. **Endpoint** : `POST /api/agenda/suggest-slot` (roles: Vet, Receptionist, Admin)
   - Retourne les 3 meilleurs creneaux tries par score decroissant

5. **Configuration** : poids du scoring configurables via `appsettings.json` section `Agenda:SlotScoring`

### Regles metier UAE (PO)

- Respecter la semaine UAE (dimanche-jeudi pour la plupart, configurable par clinique)
- Respecter les horaires Ramadan si configures
- Ne proposer que des creneaux dans les heures d'ouverture configurees de la clinique

### Feature file

`tests/Vetolib.Tests.Acceptance/Features/Agenda/SlotSuggestion.feature`

## Critere

```
[] Feature file SlotSuggestion.feature cree et steps RED
[] SuggestSlotQuery + Handler implementes
[] SlotScoringService avec 5 criteres ponderes
[] DurationEstimator avec fallback
[] Endpoint POST /api/agenda/suggest-slot
[] Poids configurables via appsettings.json
[] Tests unitaires edge cases (journee pleine, pas d'historique, un seul vet)
[] Tous les Gherkins GREEN
[] Renommer en done
```
