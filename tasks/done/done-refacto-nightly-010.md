# todo-refacto-nightly-010 — MedicalRecords: ListMedicalRecords sans pagination

**Module** : MedicalRecords
**Priorité** : HAUTE — scalabilité
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`ListMedicalRecordsHandler` charge **tous** les dossiers médicaux d'un patient sans aucune limite (`.ToListAsync(ct)` sans `.Take()`). Un patient ancien peut avoir des dizaines de dossiers avec des prescriptions incluses via `.Include()`. En production UAE avec plusieurs années de données, cela peut charger plusieurs centaines d'entités en mémoire par requête.

Fichier : `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/ListMedicalRecords/ListMedicalRecordsHandler.cs`

## Fix attendu

Ajouter une pagination côté SQL (`.Skip()/.Take()`) avec un défaut de 20 enregistrements et une limite max de 100.

### Côté query

Le `ListMedicalRecordsQuery` doit accepter des paramètres de pagination :

```csharp
// Dans Vetolib.MedicalRecords.Contracts/ ou dans le namespace interne
internal record ListMedicalRecordsQuery(Guid PatientId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<MedicalRecordDto>>>;
```

Si `PagedResult<T>` n'existe pas encore, utiliser `List<MedicalRecordDto>` avec un header `X-Total-Count` via le endpoint, ou un simple wrapper `record PagedMedicalRecordsDto(List<MedicalRecordDto> Items, int Total)`.

### Côté handler

```csharp
var pageSize = Math.Clamp(query.PageSize, 1, 100);
var page = Math.Max(1, query.Page);

var records = await _context.MedicalRecords
    .AsNoTracking()
    .Where(r => r.PatientId == query.PatientId)
    .Include(r => r.Prescriptions)
    .OrderByDescending(r => r.ExaminedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);
```

### Côté endpoint

Passer `?page=1&pageSize=20` en query string depuis `MedicalRecordEndpoints.cs`.

## Critères de complétion

```
□ .Take() appelé avant .ToListAsync()
□ Paramètres page / pageSize avec valeurs par défaut
□ PageSize clampé entre 1 et 100
□ dotnet build → 0 erreur
□ Tests acceptance MedicalRecords toujours verts
```
