# todo-refacto-nightly-006 — MedicalRecords: AsNoTracking manquant sur ListMedicalRecords, GetDrugCatalogEntryById, SearchDrugCatalog

**Module** : MedicalRecords
**Priorité** : MOYENNE — perf / change tracker inutile
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

Trois query handlers du module MedicalRecords font des lectures EF sans `.AsNoTracking()`. EF Core traque les entités chargées dans le change tracker sans qu'aucune modification ne soit jamais faite, ce qui consomme de la mémoire et du CPU inutilement.

Fichiers concernés :

- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/ListMedicalRecords/ListMedicalRecordsHandler.cs`
  — lit tous les `MedicalRecords` + `Prescriptions` d'un patient, sans AsNoTracking.
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/GetDrugCatalogEntryById/GetDrugCatalogEntryByIdHandler.cs`
  — charge une entrée de catalogue avec 3 `.Include()`, sans AsNoTracking.
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/SearchDrugCatalog/SearchDrugCatalogHandler.cs`
  — a un `.Take(limit)` correct mais pas d'AsNoTracking.

## Fix attendu

Ajouter `.AsNoTracking()` sur chaque query EF dans les 3 handlers, immédiatement après le `DbSet` (avant les `.Include()` et `.Where()`).

```csharp
// Exemple pour ListMedicalRecordsHandler
var records = await _context.MedicalRecords
    .AsNoTracking()           // ajouter ici
    .Where(r => r.PatientId == query.PatientId)
    .Include(r => r.Prescriptions)
    .OrderByDescending(r => r.ExaminedAt)
    .ToListAsync(ct);
```

Note : `.AsNoTracking()` avant `.Include()` est la position recommandée par EF Core docs pour éviter toute ambiguïté.

## Critères de complétion

```
□ AsNoTracking() présent sur les 3 handlers
□ dotnet build → 0 erreur
□ Tests acceptance MedicalRecords toujours verts
```
