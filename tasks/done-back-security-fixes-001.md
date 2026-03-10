# todo-back-security-fixes-001.md — Fixes sécurité critiques et high

**Module** : Auth + Shared
**Dépendances** : aucune
**Priorité** : CRITIQUE (Security audit C-01, H-02, H-03, H-04)

---

## Fixes à appliquer

### 1. Temp password avec crypto secure (H-02)
`InviteUserHandler.cs` utilise `new Random()`. Remplacer par :
```csharp
using System.Security.Cryptography;
var bytes = RandomNumberGenerator.GetBytes(6);
var tempPassword = Convert.ToBase64String(bytes).Replace("+", "A").Replace("/", "B")[..8] + "1a";
```

### 2. Seed passwords (H-03)
`DbInitializer.cs` a des mots de passe en dur (`Admin123!`, `Vet12345!`).
- Lire depuis les variables d'environnement ou User Secrets
- Si non défini, générer un mot de passe aléatoire et le logger une seule fois au démarrage
- Documenter dans README

### 3. PasswordHash dans audit log (H-04)
`AuditSaveChangesInterceptor` capture les `NewValues` JSON incluant `PasswordHash`.
- Exclure les propriétés sensibles : `PasswordHash`, `SecurityStamp`, `RefreshToken`
- Ajouter une liste noire dans l'intercepteur

### 4. DbInitializer jamais appelé (V-008)
Vérifier que `Program.cs` appelle bien `DbInitializer.MigrateAllAsync()` et `DbInitializer.SeedAsync()`.

### 5. DashboardEndpoints jamais mappé (V-009)
Vérifier que `app.MapDashboardApiEndpoints()` est bien appelé dans `Program.cs`.

## Critère
```
□ RandomNumberGenerator pour temp passwords
□ Seed passwords depuis env vars ou générés
□ PasswordHash exclu de l'audit log
□ DbInitializer appelé au démarrage
□ DashboardEndpoints mappé
□ dotnet build → 0 erreur
□ Renommer en done
```
