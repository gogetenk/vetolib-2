# todo-refacto-nightly-008 — Agenda: SuggestSlotHandler — N+1 + AsNoTracking

**Module** : Agenda
**Priorité** : HAUTE — perf + correctness
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`SuggestSlotHandler` (106 lignes) présente deux problèmes de performance :

### 1. Pattern N+1 implicite dans le foreach

Lignes 68-74 : une boucle `foreach (var vetSchedule in vetSchedules)` appelle `_durationEstimator.EstimateAsync()` pour chaque vétérinaire. Si `DurationEstimator` fait une requête EF à chaque appel (ce qui est le cas — il appelle `.ToListAsync(ct)` sur `_context.Appointments` line 41 de `DurationEstimator.cs`), on obtient **une requête SQL par vétérinaire** au lieu d'une seule.

Fichier : `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/SuggestSlot/SuggestSlotHandler.cs`
Service : `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Services/DurationEstimator.cs`

### 2. AsNoTracking manquant

La query ligne 32 (`.Where(a => a.Date == ...).ToListAsync(ct)`) n'a pas `.AsNoTracking()` alors que ces données sont read-only.

## Fix attendu

**Pour le N+1 :** Pré-charger les durées pour tous les vétérinaires en une seule passe avant la boucle, ou modifier `DurationEstimator` pour accepter un dictionnaire de données déjà chargées.

Option recommandée — batch dans le handler :
```csharp
// Charger les durées pour tous les vets en une passe avant la boucle
var durationByVet = new Dictionary<Guid, int>();
foreach (var vetId in vetIds)
{
    var r = await _durationEstimator.EstimateAsync(vetId, query.ConsultationType, ct);
    durationByVet[vetId] = r.IsSuccess ? r.Value : 30;
}
// Ensuite la boucle foreach n'appelle plus EstimateAsync
```

Note : si `DurationEstimator` accepte de prendre les appointments déjà chargés en paramètre, c'est encore mieux (évite la requête SQL dans le service).

**Pour AsNoTracking :** Ajouter `.AsNoTracking()` ligne 32 avant le `.Where()`.

## Critères de complétion

```
□ AsNoTracking() présent sur la query principale
□ Nombre de requêtes SQL proportionnel à 1 (pas au nombre de vets)
□ dotnet build → 0 erreur
□ Tests acceptance Agenda toujours verts
```
