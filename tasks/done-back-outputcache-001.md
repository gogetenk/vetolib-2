# todo-back-outputcache-001.md — OutputCache InMemory sur les API

**Module** : Infrastructure + tous les modules
**Dépendances** : aucune
**Priorité** : HAUTE (performance)
**Skills à lire** : `aspnet-minimal-api`

---

## Contexte

Zéro caching actuellement — chaque requête GET tape la DB. ASP.NET Core 10 fournit `OutputCache` natif avec support InMemory.

## Stack

```csharp
// Pas de NuGet supplémentaire — OutputCache est built-in .NET 8+
builder.Services.AddOutputCache(options => { ... });
app.UseOutputCache(); // après UseRouting, avant MapEndpoints
```

## Policies de cache

### Multi-tenancy : VaryBy ClinicId

Toutes les réponses sont tenant-specific. Le ClinicId vient du JWT (claim `clinic_id`).
Créer un middleware léger qui copie le claim en header pour le VaryBy :

```csharp
// Middleware ou OutputCache policy custom
app.Use(async (context, next) =>
{
    var clinicId = context.User?.FindFirst("clinic_id")?.Value;
    if (!string.IsNullOrEmpty(clinicId))
        context.Request.Headers["X-Clinic-Id"] = clinicId;
    await next();
});
```

### Policies par ressource

```csharp
builder.Services.AddOutputCache(options =>
{
    // Données stables — 5 min
    options.AddPolicy("Stable5min", builder =>
        builder.Expire(TimeSpan.FromMinutes(5))
               .SetVaryByHeader("Authorization", "X-Clinic-Id")
               .Tag("stable"));

    // Données modérément stables — 2 min
    options.AddPolicy("Moderate2min", builder =>
        builder.Expire(TimeSpan.FromMinutes(2))
               .SetVaryByHeader("Authorization", "X-Clinic-Id")
               .Tag("moderate"));

    // Dashboard — 1 min (rafraîchissement acceptable)
    options.AddPolicy("Dashboard1min", builder =>
        builder.Expire(TimeSpan.FromMinutes(1))
               .SetVaryByHeader("Authorization", "X-Clinic-Id")
               .Tag("dashboard"));

    // Pas de cache par défaut — opt-in uniquement
    options.AddBasePolicy(builder => builder.NoCache());
});
```

### Mapping endpoints → policies

| Endpoint | Policy | Durée | VaryBy supplémentaire | Tag |
|----------|--------|-------|-----------------------|-----|
| `GET /api/v1/invoices` | Stable5min | 5 min | — | invoices |
| `GET /api/v1/invoices/{id}` | Stable5min | 5 min | route:id | invoices |
| `GET /api/users` | Stable5min | 5 min | — | users |
| `GET /api/v1/patients/{id}` | Moderate2min | 3 min | route:id | patients |
| `GET /api/v1/patients/{id}/detail` | Moderate2min | 3 min | route:id | patients |
| `GET /api/v1/patients` | Moderate2min | 2 min | query:name,species,page,pageSize | patients |
| `GET /api/v1/patients/{id}/records` | Moderate2min | 2 min | route:patientId | medical-records |
| `GET /api/dashboard/stats` | Dashboard1min | 1 min | — | dashboard |
| `GET /api/dashboard/today-appointments` | Dashboard1min | 1 min | — | dashboard |
| **PAS DE CACHE** : `/api/auth/me`, `/api/v1/appointments/availability`, `/api/dashboard/recent-activity` |

### Application sur les endpoints

```csharp
// Exemple dans InvoiceEndpoints.cs
group.MapGet("/", async (...) => ...)
    .CacheOutput("Stable5min");

group.MapGet("/{id:guid}", async (...) => ...)
    .CacheOutput("Stable5min");
```

### Invalidation sur les commandes

Quand une commande modifie des données, invalider le cache par tag :

```csharp
// Injecter IOutputCacheStore dans les handlers de commande
public class CreateInvoiceHandler(BillingDbContext db, IOutputCacheStore cache, ...)
{
    public async Task<Result<Guid>> Handle(...)
    {
        // ... create invoice ...
        await cache.EvictByTagAsync("invoices", cancellationToken);
        await cache.EvictByTagAsync("dashboard", cancellationToken);
        return Result.Success(invoice.Id);
    }
}
```

**Mapping invalidation :**

| Commande | Tags à invalider |
|----------|-----------------|
| CreateAppointment | dashboard |
| UpdateAppointmentStatus | dashboard |
| EditAppointment | dashboard |
| CreatePatient | patients, dashboard |
| UpdatePatient | patients |
| AddMedicalRecord | medical-records, patients |
| AddPrescription | medical-records |
| CreateInvoice | invoices, dashboard |
| AddInvoiceItem | invoices |
| UpdateInvoiceStatus | invoices, dashboard |
| CreateUser / InviteUser | users |
| ChangeUserRole | users |
| DeactivateUser | users |

## VaryBy Authorization header

Important : le `VaryByHeader("Authorization")` garantit que chaque utilisateur a son propre cache. C'est nécessaire car :
- Les rôles déterminent ce qu'on voit (un Admin voit tout, un Assistant voit moins)
- Le ClinicId filtre par tenant
- Les deux sont dans le JWT → VaryBy Authorization couvre les deux

Alternative plus fine : `VaryByHeader("X-Clinic-Id")` + VaryByRouteValue si on veut partager le cache entre utilisateurs d'une même clinique (performance > isolation).

## Critère

```
□ AddOutputCache configuré dans Program.cs
□ UseOutputCache dans le pipeline (après auth, avant endpoints)
□ 3 policies : Stable5min, Moderate2min, Dashboard1min
□ VaryBy Authorization + X-Clinic-Id sur toutes les policies
□ CacheOutput appliqué sur les 9 endpoints GET identifiés
□ Invalidation par tag dans les handlers de commande
□ /api/auth/me et /api/v1/appointments/availability NON cachés
□ dotnet build → 0 erreur
□ Test : 2 GET consécutifs identiques → 2ème servie depuis le cache (vérifier header)
□ Renommer en done
```
