# todo-front-messaging-playwright-001.md — Tests Playwright messaging

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-inbox-001, todo-front-messaging-conversation-001, todo-front-messaging-owner-portal-001
**Priorite** : HAUTE
**Skills a lire** : `playwright-e2e`
**[MSW: oui]**

---

## Objectif

Creer les tests Playwright pour valider le comportement UI complet de la messagerie (staff inbox, conversation detail, portail owner).

## Spec de reference

`docs/MESSAGING-SPEC.md` section 3 (User Workflows Gherkin) -- version frontend des scenarios

## Implementation

### 1. Tests inbox staff

Fichier : `e2e/integration/messaging/inbox.spec.ts`

Scenarios :
- Receptionist sees only appointment/administrative messages
- Vet sees only medical messages
- Admin sees all conversations
- Assistant sees non-medical conversations in read-only
- Conversations sorted by priority then date
- Filter by status works
- Filter by category works
- Unread count badge displayed
- Search by text works

### 2. Tests conversation detail

Fichier : `e2e/integration/messaging/conversation.spec.ts`

Scenarios :
- Message thread displayed chronologically
- Internal notes visible to vet/admin, hidden from assistant
- AI suggestions panel displayed with 1-3 suggestions
- Clicking suggestion pre-fills reply textarea
- AI disclaimer displayed on suggestions panel
- Patient context panel shows correct info based on role
- Send reply updates thread
- Add internal note visible in thread with distinct style
- Transfer conversation (dialog + confirmation)
- Convert to appointment opens appointment form pre-filled
- Mark as spam removes from inbox
- Change status (resolve, close, reopen)
- Conversation summary displayed for threads > 5 messages

### 3. Tests portail owner

Fichier : `e2e/integration/messaging/portal.spec.ts`

Scenarios :
- Owner opens portal with valid magic link
- Expired magic link shows error message
- Consent screen displayed on first visit
- Accept consent allows composing message
- Pet selector shows registered pets
- Category selector available
- Character counter shows remaining (2000 max)
- Send button disabled when > 2000 chars
- Photo upload with preview
- Cannot attach more than 3 photos
- Daily message limit (5) enforced
- Conversation history displayed without internal notes
- Export button downloads text file

### 4. Tests admin settings

Fichier : `e2e/integration/messaging/admin-settings.spec.ts`

Scenarios :
- Template CRUD (create, edit, delete)
- Messaging hours configuration
- Stats dashboard displays metrics

### 5. Fixtures

Creer `e2e/fixtures/messaging.ts` :
- `loginAsReceptionist()`, `loginAsVet()`, `loginAsAdmin()`, `loginAsAssistant()`
- `navigateToInbox()`
- `openPortalWithMagicLink(token)`

## Regles

- Tests contre MSW (next dev), pas le vrai backend
- Pas de `page.route()` pour mocker -- MSW gere ca
- `data-testid` utilise pour tous les selecteurs
- Chaque test est independant (pas de dependance entre tests)

## Critere

```
[] Tests inbox staff (9 scenarios)
[] Tests conversation detail (13 scenarios)
[] Tests portail owner (13 scenarios)
[] Tests admin settings (3 scenarios)
[] Fixtures de test creees
[] Tous les tests passent avec MSW
[] npx playwright test passe
[] Renommer en done
```
