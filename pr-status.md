# pr-status.md — État des Pull Requests

> Fichier de coordination entre agents QA, PR Reviewer, PO.
> L'orchestrateur lit ce fichier pour savoir quoi lancer.

## Statuts possibles
- `[DEV_DONE]` → PR ouverte, en attente de QA et Review (parallèle)
- `[QA_FAILED]` → Tests rouges, retour au dev
- `[QA_DONE]` → Tests verts, vidéo attachée, en attente PO
- `[REVIEW_CHANGES]` → Comments PR Reviewer, en attente corrections dev
- `[REVIEW_APPROVED]` → Review technique OK
- `[PO_REJECTED]` → PO a refusé, retour au dev avec motif
- `[PO_APPROVED]` → PO a validé, prête pour merge humain
- `[MERGED]` → Mergée

---

### PR #1 — [DEV_DONE]
**Tâche** : tasks/done-back-patients-001.md
**Module** : MedicalRecords
**Branche** : feat/back-patients-001
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : en attente
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Gherkins couverts** :
- Vet creates a patient with owner inline
- Camel is a valid species for UAE market
- Patient list can be filtered by name
- Vet can update patient phone number
- Receptionist cannot create patients
- Tenant isolation on patients
- Patient detail includes medical records

**Tests unitaires** : 8 tests verts (PatientDomainTests)

---

### PR #2 — [DEV_DONE]
**Tâche** : tasks/done-wire-patients-001.md
**Module** : MedicalRecords (Patients)
**Branche** : master
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : N/A — wire annulé (Option B, cf. questions/wire-medical-001-20260309.md)
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Décision** : MSW handlers conservés pour MVP. 43 tests Playwright verts contre MSW.

---

### PR #3 — [MERGED]
**Tâche** : tasks/done-agenda-002.md
**Module** : Agenda
**Branche** : master (commit 6da39e4)
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : N/A
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Gherkins couverts** :
- Enregistrer l'arrivée du patient (check-in)
- Démarrer la consultation
- Terminer la consultation
- Annuler un rendez-vous avec motif
- Transition invalide refusée
- Consulter les disponibilités d'un vétérinaire

**Fix appliqué** : step definitions AppointmentSteps.cs alignées sur CreateAppointmentRequest v2
(PatientName/VetId/ScheduledAt) et routes `/api/appointments` — build 0 erreur.

---

### PR #4 — [DEV_DONE]
**Tâche** : tasks/done-back-messaging-owner-portal-001.md
**Module** : Messaging
**Branche** : feat/back-patients-001
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : en attente
**Ouvert le** : 2026-03-10
**Dernière activité** : 2026-03-10

**Gherkins couverts** :
- Owner sends first message (with consent pre-accepted)
- Attachment size limits enforced
- Consent required before messaging
- Daily message limit (5/day) enforced
- Out-of-hours auto-acknowledgment for non-urgent messages
- Export conversation history (PDPL)
- Expired magic link token rejected
- Urgent messages bypass out-of-hours check
- Subject derived from body when not provided
- Category-based SLA + routing assigned

**Fixes inclus** :
- `DrugInteractionSteps.cs` line 185: curly quotes replaced with escaped double quotes (build error)
- `PatientReader.cs`: `GetPatientContextAsync` implemented (missing IPatientReader method)
- `CreateOwnerConversationHandler` returns `CreateOwnerConversationResponse` with SLA + routing
- `PortalEndpoints.cs`: added `/api/v1/portal/test-token` seeding endpoint and `/api/v1/portal/categories`
- `InvoiceDto` / `InvoiceItemDto`: fields aligned with test expectations
- `FacturationSteps.cs`: field names updated (`Subtotal`, `VatAmount`)

**Tests unitaires** : N/A (handler logic covered by BDD acceptance tests)

---

### PR #5 — [DEV_DONE]
**Tâche** : tasks/done-front-messaging-playwright-001.md
**Module** : Messaging (Frontend)
**Branche** : feat/back-patients-001
**Lien PR** : local-only
**Vidéo démo** : en attente
**Ouvert le** : 2026-03-10
**Dernière activité** : 2026-03-10

**Tests couverts** : 42 tests Playwright GREEN contre MSW
- Staff Inbox (9 scénarios): RBAC par rôle, tri, filtres statut/catégorie, badge non-lu, recherche
- Conversation Detail (13 scénarios): thread, notes internes, suggestions AI, panel patient, envoi, transfert, spam, statuts, résumé
- Owner Portal (13 scénarios): magic link, consent, formulaire, photos, limite quotidienne, historique, export
- Admin Settings (3 scénarios): template CRUD, heures de messagerie, stats

**Fixes techniques** :
- `next.config.ts`: ajout de `createNextIntlPlugin` pour activer les routes locale `/en/*`
- `middleware.ts`: whitelist routes `/portal/` (publiques, pas d'`access_token`)
- `e2e/fixtures/messaging.ts`: `addInitScript` pour `portal_token` avant navigation (race condition)
- `inbox.spec.ts`: suppression de `waitForLoadState('networkidle')` incompatible avec SSE MSW
- `conversation.spec.ts`: `.first()` sur testids dupliqués (desktop + mobile panels)
- `portal.spec.ts`: `selectOption` pour CategorySelector, loop variable capture corrigée

---

## Template
### PR #{num} — [{statut}]
**Tâche** : tasks/{id}.md  
**Module** : Auth / Agenda / MedicalRecords / Billing  
**Branche** : feat/{module}-{task-id}  
**Lien PR** : {url}  
**Vidéo démo** : {url ou "en attente"}  
**Ouvert le** : {timestamp}  
**Dernière activité** : {timestamp}  
