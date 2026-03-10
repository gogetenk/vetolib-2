# todo-back-silent-failures-001.md — Fix silent failures critiques et high

**Module** : Tous
**Dépendances** : aucune
**Priorité** : CRITIQUE
**MODIF_SHARED: autorisé**

---

## Fixes CRITIQUES

### 1. Dashboard silently returns zeros on failure
`DashboardEndpoints.cs:42-44` — remplace les erreurs par des 0.

**Correction :**
- Logger un warning quand une sub-query échoue
- Retourner `Result.Error()` si une query critique échoue (pas des faux 0)

### 2. GetCurrentUserId returns Guid.Empty → auth bypass
`UserEndpoints.cs:106-109` — Guid.Empty bypass les guards self-protection.

**Correction :**
- Retourner `Result.Unauthorized()` quand le parsing du claim échoue
- Ne jamais passer Guid.Empty à un handler

### 3. TOCTOU race condition — double booking
`CreateAppointmentHandler.cs:41-79` et `EditAppointmentHandler.cs:49-61`

**Correction :**
- Ajouter un unique constraint DB sur `(VeterinarianId, Date, StartTime)` pour les RDV non-annulés
- Ajouter un retry-on-conflict (pattern identique à CreateInvoiceHandler)

### 4. ClinicContext Guid.Empty sans logging
`ClinicContext.cs:13-22` — déjà couvert par security-fixes-002 (H-03)

## Fixes HIGH

### 5. Missing validators (7 commands)
RefreshTokenCommand, LogoutCommand, ChangeUserRoleCommand, DeactivateUserCommand, UpdateAppointmentStatusCommand, EditAppointmentCommand, UpdateInvoiceStatusCommand.

**Correction :** Créer un validator pour chaque (même minimal : vérifier les Guid non-empty, les enums dans le range).

### 6. Revoke() Result ignoré (3 handlers)
LogoutHandler:25, ChangePasswordHandler:36, RefreshTokenHandler:31.

**Correction :** Vérifier `IsSuccess` et logger un warning si la révocation échoue.

### 7. ListInvoicesHandler — unbounded query
Pas de pagination, charge toutes les factures.

**Correction :** Ajouter pagination (page/pageSize) comme ListPatientsHandler.

### 8. ListAppointmentsHandler — no limit
Bounded par jour mais pas de limit.

**Correction :** Ajouter `.Take(100)` comme safety net, ou pagination.

### 9. MigrateAllAsync — no error context
`DbInitializer.cs:25-33` — pas de log sur quel context échoue.

**Correction :** Try-catch avec log du nom du DbContext avant re-throw.

## Fixes MEDIUM (best-effort)

### 10. ValidationBehavior fallthrough
Si TResponse n'est pas Result, validation ignorée.

**Correction :** Throw `InvalidOperationException` dans le fallthrough.

### 11. AuditSaveChangesInterceptor hash collision
`GetHashCode()` comme clé de ConcurrentDictionary.

**Correction :** Utiliser `RuntimeHelpers.GetHashCode()` ou `ConditionalWeakTable`.

### 12. Owner.UpdatePhone/UpdateName — no validation
Void methods, NullReferenceException possible.

**Correction :** Retourner Result, ajouter null guards.

### 13. ChangeRole — no domain validation
User.ChangeRole() toujours success, pas de vérification enum range.

**Correction :** Valider que le rôle est dans l'enum, vérifier règles métier (ex: Vet perd sa licence si changé en Receptionist?).

## Critère

```
□ Dashboard : log + error au lieu de faux zéros
□ GetCurrentUserId : Result.Unauthorized si parsing échoue
□ Unique constraint appointment (VetId, Date, StartTime) + retry
□ 7 validators créés
□ Revoke() Result vérifié
□ ListInvoices paginé
□ ListAppointments limité
□ MigrateAllAsync avec logging contextualisé
□ ValidationBehavior fallthrough → throw
□ AuditInterceptor hash collision fix
□ Owner.Update* avec Result + null guards
□ ChangeRole avec validation
□ dotnet build → 0 erreur
□ Renommer en done
```
