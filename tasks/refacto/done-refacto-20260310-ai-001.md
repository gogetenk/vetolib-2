# todo-refacto-20260310-ai-001 — Filtre ClinicId manuel redondant dans AcceptTriageHandler et OverrideTriageHandler
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/AcceptTriage/AcceptTriageHandler.cs` (ligne 20)
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/OverrideTriage/OverrideTriageHandler.cs` (ligne 20)
**Violation** : Regle 4 de CLAUDE.md — Multi-tenancy Global Query Filter. Le `AIDbContext` herite de `MultiTenantDbContext` qui applique automatiquement `WHERE ClinicId = @current`. Les handlers ajoutent manuellement `&& t.ClinicId == cmd.ClinicId` dans le `FirstOrDefaultAsync`, ce qui est redondant et viole la convention.
**Correction attendue** :
1. Retirer le filtre `&& t.ClinicId == cmd.ClinicId` des deux handlers
2. Simplifier la requete en `.FirstOrDefaultAsync(t => t.Id == cmd.TriageId, ct)`
3. Retirer la propriete `ClinicId` des commands `AcceptTriageCommand` et `OverrideTriageCommand` si elle n'est plus utilisee ailleurs
4. Adapter `AIEndpoints.cs` pour ne plus passer `clinicContext.ClinicId` a ces commands
**Critere** : grep `ClinicId == cmd.ClinicId` dans le module AI ne retourne plus de resultats
