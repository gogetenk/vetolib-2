# todo-back-audit-001.md — Audit trail (journal des modifications)

**Module** : Shared (intercepteur EF) + tous les modules
**Dépendances** : done-back-migrations-001
**Skills à lire** : `multitenant-efcore`, `ardalis-result`
**MODIF_SHARED: autorisé** (ajout de l'intercepteur d'audit dans Shared.Infrastructure)

---

## Contexte

Aucune trace de qui a modifié quoi dans le système. Pour un logiciel qui gère des dossiers médicaux et de la facturation, c'est un problème de conformité et de confiance client.

## Périmètre exact

### 1. Table AuditLog

```csharp
public class AuditEntry
{
    public long Id { get; set; }
    public string EntityType { get; set; }    // "Appointment", "Invoice", etc.
    public string EntityId { get; set; }       // Guid en string
    public string Action { get; set; }         // "Created", "Updated", "Deleted"
    public string? ChangedBy { get; set; }     // Email de l'utilisateur
    public Guid ClinicId { get; set; }         // Isolation tenant
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; }     // JSON des anciennes valeurs (null si Created)
    public string? NewValues { get; set; }     // JSON des nouvelles valeurs
}
```

### 2. Intercepteur EF Core (SaveChanges)

Créer un `AuditSaveChangesInterceptor` dans `Shared.Infrastructure` qui intercepte `SaveChangesAsync` :

```csharp
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct)
    {
        var context = eventData.Context;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is BaseEntity); // Seulement les entités métier

        foreach (var entry in entries)
        {
            // Capturer old/new values en JSON
            // Créer un AuditEntry
            // L'ajouter au contexte (sera persisté dans le même SaveChanges)
        }
    }
}
```

### 3. AuditDbContext

Un DbContext dédié `AuditDbContext` dans `Shared.Infrastructure` avec juste la table `AuditLog`. Enregistré dans chaque module via l'intercepteur.

Alternative plus simple : ajouter `DbSet<AuditEntry>` dans chaque `MultiTenantDbContext` (pas de contexte séparé). À évaluer selon la complexité.

### 4. Endpoint de consultation

```csharp
// GET /api/audit?entityType=Appointment&entityId={id}&from=2026-03-01&to=2026-03-09
// Retourne les entrées d'audit filtrées
// Accessible uniquement par ADMIN
```

Cet endpoint vit dans `Vetolib.Api` (cross-module) ou dans un nouveau module `Audit`.

### 5. Ne PAS auditer les lectures

Seuls les Create/Update/Delete sont tracés. Les GET ne génèrent pas d'entrée d'audit (trop de volume, peu de valeur).

### 6. Ne PAS auditer les tokens

Les RefreshToken et les tentatives de login ne sont pas auditées dans cette table (déjà loggées par le système d'auth). Exclure `RefreshToken` de l'intercepteur.

## Critère de complétion

```
□ Table AuditLog créée + migration
□ Intercepteur capture Create/Update/Delete sur toutes les entités métier
□ OldValues/NewValues en JSON lisible
□ ChangedBy rempli depuis le JWT (email de l'utilisateur courant)
□ ClinicId rempli (isolation multi-tenant)
□ Endpoint GET /api/audit (ADMIN only)
□ RefreshToken exclu de l'audit
□ Test unitaire : vérifier qu'un Create génère une entrée
□ Renommer en done-back-audit-001.md
```
