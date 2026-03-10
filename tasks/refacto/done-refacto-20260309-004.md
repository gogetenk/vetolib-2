# todo-refacto-20260309-004 — ValidationBehavior duplique dans 4 modules
**Priorite** : mineure
**Fichiers concernes** :
- `src/Modules/Auth/Vetolib.Auth/Application/Behaviors/ValidationBehavior.cs`
- `src/Modules/Agenda/Vetolib.Agenda/Application/Behaviors/ValidationBehavior.cs`
- `src/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Behaviors/ValidationBehavior.cs`
- `src/Modules/Billing/Vetolib.Billing/Application/Behaviors/ValidationBehavior.cs`
**Violation** : Duplication de code — le meme `ValidationBehavior<TRequest, TResponse>` est copie-colle dans chaque module
**Contexte** : La regle d'isolation modulaire interdit les references cross-runtime, ce qui explique la duplication. Cependant, ce behavior est purement infrastructure (pas de logique metier) et pourrait etre mutualise dans Shared.
**Correction attendue** :
- Deplacer `ValidationBehavior` dans `Shared/Vetolib.Shared.Infrastructure/` (necessite autorisation car Shared est GELE)
- Chaque module le consomme via la reference Shared.Infrastructure deja existante
- Supprimer les 4 copies dans les modules
**Critere** : [] Un seul `ValidationBehavior` existe dans Shared, les 4 copies modules sont supprimees
