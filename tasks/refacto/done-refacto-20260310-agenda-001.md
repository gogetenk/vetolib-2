# todo-refacto-20260310-agenda-001 — IgnoreQueryFilters dans AppointmentReminderService
**Priorite** : critique
**Fichiers concernes** : `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs` (ligne 63)
**Violation** : Regle de CLAUDE.md — `IgnoreQueryFilters` est interdit en production (sauf migrations/seeds). Le reminder service utilise `IgnoreQueryFilters()` pour envoyer des rappels cross-tenant. Ce service tourne en arriere-plan sans contexte de clinic, ce qui explique l'usage, mais cela n'est pas documente dans `disputes.md` et constitue une violation architecturale.
**Correction attendue** :
- Option A (recommandee) : Documenter cette exception dans `disputes.md` avec justification (service de fond sans contexte tenant), ajouter un commentaire `// EXCEPTION: background service — approved in disputes.md`
- Option B : Creer un scope par clinic et iterer sur les clinics actives pour respecter le tenant filter
**Critere** : L'exception est documentee dans `disputes.md` OU le `IgnoreQueryFilters` est supprime
