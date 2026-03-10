# todo-refacto-20260310-messaging-008 -- IMessageRouter et ITriageOrchestrator non enregistres dans le DI
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/MessagingModuleServiceRegistrar.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/CreateOwnerConversation/CreateOwnerConversationHandler.cs`
**Violation** : Le `CreateOwnerConversationHandler` injecte `ITriageOrchestrator` et `IMessageRouter` dans son constructeur (lignes 42-43), mais aucun de ces deux services n'est enregistre dans `MessagingModuleServiceRegistrar.AddMessagingModule()`. Seuls `IBusinessHoursChecker`, `IMessagingEventBroadcaster` et `IPortalContext` sont enregistres. Cela provoquera une `InvalidOperationException` au runtime lors de la resolution DI du handler.
**Correction attendue** :
1. Ajouter dans `AddMessagingModule()` :
   - `services.AddScoped<IMessageRouter, MessageRouter>();`
   - `services.AddScoped<ITriageOrchestrator, TriageOrchestrator>();`
2. Verifier que le handler fonctionne en executant le test BDD de creation de conversation owner
**Critere** : `grep -r "IMessageRouter\|ITriageOrchestrator" MessagingModuleServiceRegistrar.cs` retourne les 2 enregistrements
