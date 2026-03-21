# todo-back-security-fixes-002.md — Fixes sécurité round 2

**Module** : Auth + Audit + Shared
**Dépendances** : aucune
**Priorité** : CRITIQUE (Security audit C-01, C-02, C-03, H-01, H-03, H-04)
**MODIF_SHARED: autorisé**

---

## Fixes à appliquer

### 1. C-01 : Seed passwords logged in plaintext
`DbInitializer.cs` — les mots de passe générés sont loggés via `logger.LogWarning`.

**Correction :**
- En production (`!IsDevelopment()`), EXIGER `Seed:AdminPassword` et `Seed:VetPassword` — fail-fast si absent
- En dev, logger le mot de passe UNIQUEMENT sur la console locale, pas en structured logging
- Remplacer `{AdminPassword}` (paramètre structuré) par une concaténation pour éviter la sérialisation JSON

### 2. C-02 : Temporary password returned in API response
`InviteUserResponse.cs` contient `TemporaryPassword` — visible dans la réponse HTTP.

**Correction :**
- Supprimer `TemporaryPassword` de `InviteUserResponse`
- Retourner uniquement `(Id, Email, FullName, Role)`
- Le mot de passe est déjà envoyé par email via `UserInvitedDomainEvent`

### 3. C-03 : Audit endpoint sans tenant filtering
`AuditEndpoints.cs` + `AuditDbContext` — pas de filtre ClinicId.

**Correction :**
- Injecter `IClinicContext` dans le handler audit
- Ajouter `WHERE ClinicId = @currentClinicId` dans la query
- Ou ajouter un global query filter sur `AuditDbContext` (mais il n'hérite pas de MultiTenantDbContext)

### 4. H-01 : Rate limiting non appliqué sur les endpoints non-auth
Seuls les endpoints Auth ont RequireRateLimiting.

**Correction :**
- Appliquer `RequireRateLimiting("api")` comme middleware global OU sur chaque groupe d'endpoints
- Alternative : configurer un `GlobalLimiter` dans `AddRateLimiter` qui s'applique par défaut

### 5. H-03 : ClinicContext returns Guid.Empty
`ClinicContext.cs` retourne `Guid.Empty` quand le claim est absent.

**Correction :**
- Si HttpContext existe ET l'utilisateur est authentifié MAIS clinic_id absent → throw
- Si pas de HttpContext (background service) → retourner Guid.Empty (acceptable pour les services)

### 6. H-04 : Password policy trop faible
Min 8 chars, 1 uppercase, 1 digit — pas de caractère spécial.

**Correction :**
- Ajouter validation : au moins 1 caractère spécial (!@#$%^&*...)
- Augmenter la longueur minimale à 10 caractères
- Mettre à jour les validators FluentValidation correspondants

## Critère

```
□ Seed passwords : fail-fast en prod, console-only en dev
□ InviteUserResponse : TemporaryPassword supprimé
□ Audit endpoint : filtré par ClinicId
□ Rate limiting global sur tous les endpoints
□ ClinicContext : throw si authentifié sans clinic_id
□ Password policy : 10 chars min + 1 special char
□ dotnet build → 0 erreur
□ Renommer en done
```
