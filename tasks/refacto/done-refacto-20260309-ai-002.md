# todo-refacto-20260309-ai-002 -- IAITriageService non enregistree en DI
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/AI/Vetolib.AI.Contracts/IAITriageService.cs`
- `src/backend/Modules/AI/Vetolib.AI/ModuleServiceRegistrar.cs`
**Violation** : L'interface `IAITriageService` est declaree dans Contracts avec une signature `Task<Result<TriageSuggestionDto>> TriageAsync(...)`, mais aucune classe ne l'implemente et elle n'est pas enregistree en DI. Le triage passe uniquement par le handler MediatR `TriageSymptomsHandler`. Si un autre module veut appeler le triage via Contracts (communication inter-modules), il ne peut pas.
**Correction attendue** :
- Option A : Faire implementer `IAITriageService` par une classe interne qui delegue a MediatR, et l'enregistrer en DI dans `AddAIModule()`.
- Option B : Si la communication inter-modules n'est pas prevue pour le triage, supprimer `IAITriageService` de Contracts pour eviter la confusion.
**Critere** : L'interface a une implementation enregistree en DI, OU est supprimee de Contracts.
