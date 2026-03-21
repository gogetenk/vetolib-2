# todo-back-contract-align-001.md — Aligner les contrats API frontend/backend

**Module** : Tous les modules backend + frontend lib/api
**Dépendances** : aucune
**Skills à lire** : `aspnet-minimal-api`, `msw-mock-api`

---

## Contexte

Le backend expose ses endpoints sous `/api/v1/` (versionné) tandis que le frontend appelle `/api/` (non versionné). Les types TypeScript des DTOs frontend ne correspondent pas toujours exactement aux DTOs C# backend. Ces divergences empêchent le wiring réel.

## Périmètre exact

### 1. Auditer toutes les divergences

Comparer systématiquement :

| Frontend (lib/api/*.ts) | Backend (*Endpoints.cs) | Divergence attendue |
|---|---|---|
| `POST /api/auth/login` | `POST /api/auth/login` | Vérifier champs request/response |
| `GET /api/appointments` | `GET /api/v1/appointments` | Préfixe `/api/` vs `/api/v1/` |
| `GET /api/patients` | `GET /api/v1/patients` | Préfixe + champs |
| `GET /api/invoices` | `GET /api/v1/invoices` | Préfixe + champs |
| `GET /api/users` | `GET /api/users` | Vérifier |
| `GET /api/dashboard/*` | N/A | Endpoints dashboard n'existent pas côté backend |

### 2. Choisir une convention et aligner

**Option recommandée** : le backend supprime le `/v1/` et sert tout sous `/api/`. C'est un MVP, pas besoin de versioning API.

Modifier les `MapGroup` dans chaque `*Endpoints.cs` :
```csharp
// ❌ Avant
var group = app.MapGroup("/api/v1/appointments");

// ✅ Après
var group = app.MapGroup("/api/appointments");
```

### 3. Aligner les DTOs

Pour chaque endpoint, vérifier que :
- Les noms de champs JSON correspondent (camelCase côté C# = camelCase côté TS)
- Les types correspondent (DateOnly → string ISO, Guid → string, enum → string)
- Les champs optionnels sont cohérents
- Les réponses paginées ont le même format

### 4. Créer les endpoints dashboard côté backend

Le frontend attend 3 endpoints dashboard qui n'existent pas :

```
GET /api/dashboard/stats → { appointmentsToday, pendingCheckin, unpaidInvoicesAed, totalPatients }
GET /api/dashboard/today-appointments → AppointmentDto[]
GET /api/dashboard/recent-activity → ActivityDto[]
```

Ces endpoints sont cross-module (agrègent Agenda + Billing + MedicalRecords). Ils doivent vivre dans `Vetolib.Api` directement (pas dans un module) et requérir uniquement des références aux `.Contracts`.

### 5. Documenter le contrat final

Créer un fichier `docs/api-contract.md` listant tous les endpoints avec leurs request/response types.

## Critère de complétion

```
□ Tous les endpoints backend sous /api/ (plus de /api/v1/)
□ DTOs C# et types TypeScript alignés pour chaque endpoint
□ Endpoints dashboard créés côté backend
□ Fichier docs/api-contract.md créé
□ dotnet build → 0 erreur
□ Tests Reqnroll toujours verts (URLs mises à jour)
□ Renommer en done-back-contract-align-001.md
```
