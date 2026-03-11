# todo-refacto-20260310-audit-006 -- IgnoreQueryFilters dans AppointmentReminderService sans commentaire disputes.md
**Priorite** : mineure
**Fichiers concernes** :
- `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs` (ligne 63)

**Violation** : Regle 4 CLAUDE.md -- IgnoreQueryFilters hors migrations/seeds. Le service background utilise IgnoreQueryFilters pour scanner les rappels cross-clinics. Bien que fonctionnellement justifie (background service cross-tenant), il n'y a pas de reference a disputes.md ni d'approbation explicite.
**Correction attendue** : Ajouter un commentaire `// EXCEPTION approved: cross-tenant background service (see archi-spec multi-tenancy rules)` et enregistrer dans disputes.md la decision architecturale.
**Critere** : [] Le commentaire reference disputes.md et l'exception est documentee
