# todo-refacto-20260309-agenda-001 -- SuggestSlotQuery: FluentValidation validator manquant
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/SuggestSlot/` (pas de fichier *Validator.cs)
**Violation** : Le `SuggestSlotQuery` n'a pas de `FluentValidation` validator. Le `ValidationBehavior` pipeline est en place mais aucune regle ne valide les entrees du query (ConsultationType non vide, PreferredDate dans le futur, DurationMinutes positif si fourni, etc.). Tous les autres handlers du module Agenda ont des validators (CreateAppointment, UpdateAppointmentStatus, EditAppointment).
**Correction attendue** :
1. Creer `SuggestSlotValidator.cs` dans `Application/Queries/SuggestSlot/`
2. Valider : `ConsultationType` non vide, `PreferredDate` >= aujourd'hui, `DurationMinutes` > 0 si fourni, `PreferredTime` dans les heures ouvrables
3. La classe doit etre `internal`
**Critere** : Un fichier `SuggestSlotValidator.cs` existe et valide les champs du query
