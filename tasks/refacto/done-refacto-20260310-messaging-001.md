# todo-refacto-20260310-messaging-001 — Duplicate ValidationBehavior in Messaging module
**Priorite** : critique
**Fichiers concernes** : `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Behaviors/ValidationBehavior.cs`
**Violation** : Le module Messaging contient une copie locale de `ValidationBehavior<TRequest, TResponse>` alors qu'une version centralisee existe deja dans `Vetolib.Shared.Infrastructure.Behaviors.ValidationBehavior`. Le module AI utilise correctement la version partagee. La copie locale diverge deja (manque le `throw InvalidOperationException` guard pour les types non-Result).
**Correction attendue** :
1. Supprimer `Vetolib.Messaging/Application/Behaviors/ValidationBehavior.cs`
2. Dans `MessagingModuleServiceRegistrar.cs`, remplacer `using Vetolib.Messaging.Application.Behaviors;` par `using Vetolib.Shared.Infrastructure.Behaviors;`
3. Verifier que le `Vetolib.Messaging.csproj` reference bien `Vetolib.Shared.Infrastructure` (deja le cas)
**Critere** : le dossier `Application/Behaviors/` du module Messaging n'existe plus, et le module compile avec la ValidationBehavior partagee
