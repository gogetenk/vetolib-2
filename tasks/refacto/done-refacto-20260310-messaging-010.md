# todo-refacto-20260310-messaging-010 -- IgnoreQueryFilters excessif dans les handlers portal -- documenter ou centraliser
**Priorite** : mineure
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/CreateOwnerConversation/CreateOwnerConversationHandler.cs` (4 occurrences)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/SendOwnerMessage/SendOwnerMessageHandler.cs` (3 occurrences)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/AcceptConsent/AcceptConsentHandler.cs` (1 occurrence)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListOwnerConversations/ListOwnerConversationsHandler.cs` (1 occurrence)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/GetOwnerConversationById/GetOwnerConversationByIdHandler.cs` (1 occurrence)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ExportOwnerConversations/ExportOwnerConversationsHandler.cs` (1 occurrence)
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Services/BusinessHoursChecker.cs` (1 occurrence)
**Violation** : 15 occurrences de `IgnoreQueryFilters()` dans le module Messaging. L'usage est justifie dans la plupart des cas (portal auth ne passe pas par JWT, donc le ClinicId du multi-tenant filter n'est pas defini pour les requetes portal). Cependant la justification n'est documentee que dans 2 handlers sur 7. Risque : un developpeur pourrait copier ce pattern sans comprendre pourquoi il est necessaire.
**Correction attendue** :
Option A (recommandee) : Ajouter un commentaire normalise `// IgnoreQueryFilters: portal auth bypasses JWT tenant context` sur chaque occurrence non-documentee
Option B (plus robuste) : Creer un `PortalQueryExtensions.ForPortal(ownerId, clinicId)` qui encapsule `IgnoreQueryFilters().Where(e => e.ClinicId == clinicId)` pour eviter la repetition
**Critere** : Chaque occurrence de IgnoreQueryFilters dans le module est documentee avec la raison
