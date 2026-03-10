# todo-refacto-20260309-ai-003 -- Validators FluentValidation manquants sur 4 commands
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/AcceptTriage/` (pas de validator)
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/OverrideTriage/` (pas de validator)
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/PredictNoShow/` (pas de validator)
- `src/backend/Modules/AI/Vetolib.AI/Application/Commands/PredictNoShowBatch/` (pas de validator)
**Violation** : Le pipeline MediatR inclut `ValidationBehavior<,>` (enregistre dans ModuleServiceRegistrar), mais seule `TriageSymptomsValidator` existe. Les 4 autres commands n'ont pas de validator. Meme si les validations sont simples (Guid non-vide, DateOnly valide), l'absence de validator signifie que des requetes invalides atteignent les handlers.
**Correction attendue** :
1. Creer `AcceptTriageValidator` : valider `TriageId != Guid.Empty`
2. Creer `OverrideTriageValidator` : valider `TriageId != Guid.Empty`, `NewSeverity` est defini dans l'enum
3. Creer `PredictNoShowValidator` : valider `AppointmentId != Guid.Empty`
4. Creer `PredictNoShowBatchValidator` : valider `Date` (pas dans le passe lointain, par exemple)
**Critere** : Chaque command a un validator FluentValidation correspondant.
