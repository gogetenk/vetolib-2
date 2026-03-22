# todo-refacto-20260310-audit-003 -- IgnoreQueryFilters excessif dans Messaging handlers
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListOwnerConversations/ListOwnerConversationsHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/GetOwnerConversationById/GetOwnerConversationByIdHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ExportOwnerConversations/ExportOwnerConversationsHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/SendOwnerMessage/SendOwnerMessageHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/CreateOwnerConversation/CreateOwnerConversationHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Commands/AcceptConsent/AcceptConsentHandler.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Services/EmergencyEscalationBackgroundService.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Services/BusinessHoursChecker.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/MagicLinkEndpointFilter.cs`

**Violation** : Regle 4 CLAUDE.md -- "IgnoreQueryFilters INTERDIT sauf seeds/migrations". Le module Messaging a 9 fichiers qui utilisent IgnoreQueryFilters. Le commentaire "portal auth bypasses JWT tenant context" revient partout -- cela indique un probleme architectural : le portal devrait injecter un ClinicContext avec le bon ClinicId plutot que de bypasser le filtre a chaque requete.
**Correction attendue** : Creer un `PortalClinicContext` qui resolve le ClinicId depuis le magic link token ou la conversation, et l'injecter dans le scope DI des endpoints portal. Cela eliminerait tous les IgnoreQueryFilters dans les handlers portal.
**Critere** : [] Le nombre de IgnoreQueryFilters dans Messaging est reduit a 2 maximum (BackgroundService cross-clinic est acceptable)
