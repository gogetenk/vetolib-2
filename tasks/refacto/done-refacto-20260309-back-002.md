# todo-refacto-20260309-back-002 — IgnoreQueryFilters in AppointmentReminderService (non-seed/migration)
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs` (ligne 63)
**Violation** : IgnoreQueryFilters utilise hors seeds/migrations. Le background service contourne le filtre multi-tenant pour traiter les rappels cross-clinic. C'est fonctionnellement necessaire mais viole la regle CLAUDE.md qui interdit IgnoreQueryFilters en production.
**Correction attendue** : Documenter l'exception dans un commentaire `// EXCEPTION: cross-tenant background job — approved in disputes.md` et ajouter l'approbation dans `disputes.md`. Alternativement, injecter un IClinicContext specifique "system" qui retourne Guid.Empty et adapter le filtre pour ne pas filtrer quand ClinicId == Guid.Empty.
**Critere** : Le IgnoreQueryFilters est soit elimine (via un ClinicContext system), soit formellement approuve dans disputes.md.
