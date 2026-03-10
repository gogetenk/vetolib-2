# Question — messaging-module-scope-001 [RESOLU]

**Module** : Messaging
**Bloquant** : Non (resolu)

## Decision

**Option B retenue** — Le messaging sera un module separe `Vetolib.Messaging` (avec `Vetolib.Messaging.Contracts`), et non une partie du module `Vetolib.AI`.

## Justification PO

Le brief fonctionnel (`docs/MESSAGING-AI-BRIEF.md`) decrit un produit complet a part entiere :

1. **Domaine metier propre** : entites Conversation, Message, ConversationStatus, templates de reponse, notes internes, escalade, SLA par categorie. Ce n'est pas de l'IA, c'est de la gestion de communication clinique.

2. **Portail owner avec auth separee** (magic links, pas de JWT classique). Ce mecanisme d'auth n'a rien a voir avec le module AI.

3. **Inbox priorisee par role** avec des regles metier complexes (RECEPTIONIST ne voit pas les messages medicaux, ASSISTANT en lecture seule, routage par categorie). Ces regles sont des regles metier Messaging, pas AI.

4. **Interactions cross-modules fortes** : Agenda (convertir en RDV, vet de garde), MedicalRecords (ajouter au dossier, contexte patient), Billing (contexte factures). Un module AI ne devrait pas porter ces responsabilites.

5. **Le module AI fournit un service** au module Messaging, pas l'inverse. Le triage LLM et la generation de suggestions sont des services consommes via `Vetolib.AI.Contracts` (interface `IAITriageService` ou equivalent). Le module Messaging orchestre le workflow metier et appelle l'AI quand il en a besoin.

## Perimetre des modules

- **Vetolib.AI** : triage LLM (classification + suggestion de reponse), resume de conversation, prediction no-show, triage symptomes. Expose des interfaces dans `Vetolib.AI.Contracts`.
- **Vetolib.Messaging** : entites Conversation/Message, portail owner (magic link auth), inbox par role, templates, notes internes, escalade, actions contextuelles (convertir en RDV, ajouter au dossier), notifications temps reel (WebSocket/SSE). Consomme `Vetolib.AI.Contracts` pour le triage et les suggestions.

## Impact

- Nouveau module a scaffolder (`Vetolib.Messaging` + `Vetolib.Messaging.Contracts`)
- `MessagingDbContext` propre avec ses entites
- Modification de fichiers geles (`AppHost/Program.cs`, `Vetolib.Api/Program.cs`) pour enregistrer le module — necessite autorisation humaine via `disputes.md` au moment de l'implementation
- La spec `docs/AI-FEATURES-SPEC.md` section 6 (Feature 4) doit etre mise a jour pour refleter cette separation : le module AI garde le triage/suggestion/resume, le module Messaging porte le domaine metier

→ Escalade humain requise : non
