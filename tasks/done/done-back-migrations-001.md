# todo-back-migrations-001.md — EF Core Migrations + Seed Data

**Module** : Shared / All modules
**Dépendances** : aucune
**Skills à lire** : `multitenant-efcore`, `dotnet-aspire`
**MODIF_SHARED: autorisé** (ajout du code de migration/seed uniquement)

---

## Contexte

Actuellement la DB est code-first sans aucune migration. `EnsureCreatedAsync()` crée le schéma mais ce n'est pas reproductible ni versionnable. Pour un déploiement réel, il faut des migrations EF Core et des données de seed.

## Périmètre exact

### 1. Créer les migrations initiales

Une migration par DbContext (isolation modulaire) :

```bash
dotnet ef migrations add InitialCreate --context AuthDbContext --output-dir Migrations/Auth
dotnet ef migrations add InitialCreate --context AgendaDbContext --output-dir Migrations/Agenda
dotnet ef migrations add InitialCreate --context MedicalRecordsDbContext --output-dir Migrations/MedicalRecords
dotnet ef migrations add InitialCreate --context BillingDbContext --output-dir Migrations/Billing
```

Les migrations doivent vivre dans le projet `Vetolib.Api` (ou dans un projet dédié `Vetolib.Migrations`).

### 2. Remplacer EnsureCreatedAsync par Migrate

Dans `Program.cs` du Api host, remplacer tout appel `EnsureCreatedAsync()` par `MigrateAsync()` :

```csharp
// ❌ Avant
await dbContext.Database.EnsureCreatedAsync();

// ✅ Après
await dbContext.Database.MigrateAsync();
```

### 3. Seed data — une clinique + un admin

Créer un `DbInitializer` (ou utiliser `HasData` dans les configurations EF) :

```
Clinique de démo :
  - Id: "clinic-demo-001"
  - Name: "Desert Paws Veterinary Clinic"
  - Timezone: "Asia/Dubai"

Admin initial :
  - Email: admin@desertpaws.ae
  - Password: Admin123! (hashé BCrypt)
  - FullName: "Omar Al-Rashid"
  - Role: ADMIN
  - ClinicId: clinic-demo-001

Vétérinaire démo :
  - Email: dr.sarah@desertpaws.ae
  - Password: Vet12345!
  - FullName: "Dr. Sarah Johnson"
  - Role: VET
  - ClinicId: clinic-demo-001
```

Le seed doit être **idempotent** — relancer ne duplique rien (check existence avant insert).

### 4. Vérifier que Aspire orchestre correctement

Le `AppHost/Program.cs` doit attendre que PostgreSQL soit healthy avant de lancer l'API. Vérifier que `AddNpgsqlDbContext` + les health checks fonctionnent.

## Critère de complétion

```
□ 4 migrations initiales créées (une par module)
□ dotnet ef database update fonctionne sur une DB vierge
□ Seed : clinique + admin + vet créés au premier lancement
□ Seed idempotent (relancer ne crash pas)
□ EnsureCreatedAsync supprimé partout
□ dotnet build → 0 erreur
□ Tests Reqnroll toujours verts
□ Renommer en done-back-migrations-001.md
```
