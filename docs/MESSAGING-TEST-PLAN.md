# MESSAGING-TEST-PLAN.md

**Spec**: docs/MESSAGING-SPEC.md
**Date**: 2026-03-10
**Auteur**: QA Agent (Vetolib)

> Priorites: **P0** = bloquant MVP, **P1** = important, **P2** = nice-to-have.

---

## 1. Tests BDD -- Reqnroll

### Emplacement : tests/Vetolib.Tests.Acceptance/Features/Messaging/

> MessageTriage.feature existe deja. Scenarios chevauchants marques (EXISTANT). Nouveaux a creer.

---

### 1.1 Owner Portal -- OwnerPortal.feature [P0]

Bindings : StepDefinitions/Messaging/OwnerPortalSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| OP-01 | Owner sends a new message about a health concern | P0 | HTTP 201, conversation creee status=Open |
| OP-02 | Owner sends message with 2 photo attachments (JPG < 5MB) | P0 | HTTP 201, 2 MessageAttachment en base |
| OP-03 | Owner cannot attach more than 3 photos | P0 | HTTP 422, Maximum 3 photos per message |
| OP-04 | Owner cannot send message exceeding 2000 characters | P0 | HTTP 422, validation error body.length |
| OP-05 | Owner with expired magic link is rejected | P0 | HTTP 401, This link has expired |
| OP-06 | Owner without registered pets sees only Other category | P0 | GET /api/v1/portal/pets vide, POST category=Other OK |
| OP-07 | Owner must accept consent before first message | P0 | POST sans consentement HTTP 403, apres consent 201 |
| OP-08 | Owner cannot send more than 5 messages per day | P0 | 6eme message HTTP 429 |
| OP-09 | Owner views conversation history without internal notes | P0 | IsInternalNote=true absents de la reponse |
| OP-10 | Owner downloads all conversations as text file | P1 | GET /api/v1/portal/export, Content-Type text/plain |
| OP-11 | Owner sends non-urgent message outside business hours | P0 | Message System d accuse de reception envoye |
| OP-12 | Owner sends emergency message outside business hours | P0 | MedicalUrgency creee, pas de message System, on-call notifie |
| OP-13 | Owner receives email when staff replies | P1 | OwnerMessageReplyEvent publie via MassTransit |
| OP-14 | Owner cannot reply to closed conversation | P0 | POST message sur Closed HTTP 409 |
| OP-15 | Owner message on Resolved conversation reopens it | P1 | status revient a Open |

---

### 1.2 Receptionist Inbox -- ReceptionistInbox.feature [P0]

Bindings : StepDefinitions/Messaging/ReceptionistInboxSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| RI-01 | Receptionist sees only AppointmentRequest and Administrative | P0 | Aucune MedicalQuestion ni MedicalUrgency dans la reponse |
| RI-02 | Messages sorted by priority then by date | P1 | AppointmentRequest avant Administrative, tie-break par date |
| RI-03 | Receptionist replies -- status becomes InProgress | P0 | POST /reply, status=InProgress, OwnerMessageReplyEvent publie |
| RI-04 | Receptionist uses a quick response template | P1 | GET /templates, POST /reply avec contenu template |
| RI-05 | Receptionist transfers uncertain-triage message to vet | P0 | Conversation disparait inbox receptionist, note Transferred by dans inbox vet |
| RI-06 | Receptionist converts message to appointment | P0 | CreateAppointmentFromMessageCommand publie avec champs pre-remplis |
| RI-07 | Receptionist marks message as spam | P1 | POST /spam, message disparait inbox, visible admin spam |
| RI-08 | Receptionist sees patient context without medical records | P0 | pet name, species, last appt, invoices -- pas prescriptions |
| RI-09 | Receptionist cannot add internal notes | P0 | POST /notes HTTP 403 |
| RI-10 | Triage uncertain flag visible on message | P1 | IsTriageUncertain=true dans reponse API |

---

### 1.3 Vet Inbox -- VetInbox.feature [P0]

Bindings : StepDefinitions/Messaging/VetInboxSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| VI-01 | Vet sees medical messages in priority order | P0 | Ordre : Critical, High, Normal, tie-break par date |
| VI-02 | Emergency message always at top regardless of age | P0 | MedicalUrgency 5min avant MedicalQuestion 2h |
| VI-03 | Vet sees full medical context alongside message | P0 | last exam date, prescriptions, allergies, vaccinations |
| VI-04 | Vet sees AI conversation summary for threads > 5 messages | P1 | GET /summary, 3-5 phrases factuelles, pas de diagnostic |
| VI-05 | Vet adds an internal note | P0 | POST /notes IsInternalNote=true, visible staff, absent owner |
| VI-06 | Vet reply with modified AI suggestion -- WasSuggestedReplyUsed=false | P1 | ReplyAudit cree, WasSuggestedReplyUsed=false |
| VI-07 | Vet reply with unmodified AI suggestion -- WasSuggestedReplyUsed=true | P1 | Corps identique a suggestion, WasSuggestedReplyUsed=true |
| VI-08 | Vet creates urgent appointment from message | P0 | CreateAppointmentFromMessageCommand type=Emergency |
| VI-09 | Vet attaches message content to medical record | P0 | AddMessageToRecordCommand publie, texte + photo URLs |
| VI-10 | Vet receives push notification for emergency | P0 | EmergencyMessageReceivedEvent publie via MassTransit |
| VI-11 | Emergency escalation after 10 minutes without view | P0 | EmergencyEscalationEvent publie 10min apres creation (EXISTANT a enrichir) |
| VI-12 | Vet resolves a conversation | P0 | PATCH /status, status=Resolved |

---

### 1.4 Admin Messaging Management -- AdminMessaging.feature [P0/P1]

Bindings : StepDefinitions/Messaging/AdminMessagingSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| AM-01 | Admin sees all conversations regardless of category | P0 | GET conversations sans filtre, toutes categories retournees |
| AM-02 | Admin reassigns conversation from Dr. Ahmad to Dr. Fatima | P1 | AssignedToUserId mis a jour, conversation dans inbox Dr. Fatima |
| AM-03 | Admin creates bilingual template (EN + AR) | P1 | ContentEn + ContentAr persistes, disponible via GET /templates |
| AM-04 | Admin updates messaging hours (Sun-Thu 08:00-20:00, Fri 08:00-12:00) | P0 | 7 entrees MessagingHours persistees, Sat IsClosed=true |
| AM-05 | Admin permanently closes a conversation | P0 | status=Closed, POST message HTTP 409 |
| AM-06 | Admin creates outbound conversation to owner | P1 | POST /conversations/outbound, SendMagicLinkEvent publie |
| AM-07 | Admin views spam folder | P1 | GET conversations?spam=true, liste messages marques spam |
| AM-08 | Admin restores a spam message | P1 | IsSpam=false, reapparait dans inbox normal |
| AM-09 | Admin views triage statistics | P2 | GET /stats, avg response time, messages by category, AI accuracy |

---

### 1.5 AI Triage -- MessageTriage.feature [P0] -- ENRICHISSEMENT

> MessageTriage.feature existe. Scenarios ci-dessous absents ou incorrects dans la version actuelle.

| # | Scenario | Priorite | Statut |
|---|----------|----------|--------|
| MT-01 | Emergency message classified correctly (confidence > 0.8) | P0 | EXISTANT -- corriger enum : MedicalUrgency (pas Emergency) |
| MT-02 | Appointment request classified correctly | P0 | EXISTANT |
| MT-03 | Administrative question classified correctly | P0 | EXISTANT -- corriger enum : Administrative (pas AdminQuestion) |
| MT-04 | Post-operative follow-up classified correctly | P0 | EXISTANT -- corriger enum : PostOperativeFollowUp (pas PostOpFollowUp) |
| MT-05 | Low confidence < 0.7 routes to receptionist with uncertain flag | P0 | EXISTANT |
| MT-06 | AI biases toward MedicalUrgency when uncertain between Medical types | P0 | MANQUANT |
| MT-07 | Feedback message classified and routed to admin | P1 | MANQUANT |
| MT-08 | AI suggests 1-3 replies in same language as original message | P0 | EXISTANT (manque assertion langue) |
| MT-09 | Arabic message generates Arabic suggestions | P0 | MANQUANT |
| MT-10 | AI generates summary for conversation with > 5 messages | P1 | MANQUANT |
| MT-11 | Emergency escalation after 10 min no view | P0 | EXISTANT |
| MT-12 | ReplyAudit WasSuggestedReplyUsed tracking | P1 | EXISTANT |
| MT-13 | Message linked to patient record | P0 | EXISTANT |
| MT-14 | Multi-tenant message isolation | P0 | EXISTANT |
| MT-15 | Confidence threshold boundary (0.70 uncertain, 0.71 normal) | P1 | MANQUANT |

Probleme a corriger : les valeurs d enum ne correspondent pas a la spec finale. Emergency doit etre MedicalUrgency, AdminQuestion doit etre Administrative, PostOpFollowUp doit etre PostOperativeFollowUp. Aligner bindings et feature file sur les enums de Vetolib.Messaging.Contracts.

---

### 1.6 Assistant Access -- AssistantAccess.feature [P1]

Bindings : StepDefinitions/Messaging/AssistantAccessSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| AA-01 | Assistant sees AppointmentRequest and Administrative | P1 | GET conversations, categories non-medicales retournees |
| AA-02 | Assistant does not see MedicalUrgency or MedicalQuestion | P1 | Aucune conversation medicale dans la reponse |
| AA-03 | Assistant cannot reply | P1 | POST /reply HTTP 403 |
| AA-04 | Assistant cannot transfer, convert, add note, or mark spam | P1 | Chaque action HTTP 403 |

---

### 1.7 Conversation Lifecycle -- ConversationLifecycle.feature [P0]

Bindings : StepDefinitions/Messaging/ConversationLifecycleSteps.cs

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| CL-01 | New conversation starts in Open status | P0 | POST conversation, status=Open |
| CL-02 | First staff reply moves conversation to InProgress | P0 | POST /reply, status=InProgress |
| CL-03 | Staff marks conversation as Resolved | P0 | PATCH status=Resolved OK |
| CL-04 | Owner message on Resolved conversation reopens it | P0 | POST message portal, status=Open |
| CL-05 | Admin permanently closes a conversation | P0 | status=Closed, POST message HTTP 409 |
| CL-06 | Cannot transition directly from Open to Resolved | P1 | PATCH status=Resolved sur Open, HTTP 422 |

---

### 1.8 Edge Cases -- MessagingEdgeCases.feature [P0/P1]

| # | Scenario | Priorite | Criteres |
|---|----------|----------|----------|
| EC-01 | Photo attachment exceeds 5 MB | P0 | HTTP 422, File size exceeds 5 MB |
| EC-02 | Photo attachment has invalid MIME type (PDF) | P0 | HTTP 422, Only JPG and PNG are allowed |
| EC-03 | Message body is empty | P0 | HTTP 422, validation error |
| EC-04 | UAE public holiday treated as closed day | P1 | Non-urgent message le jour Eid, accuse de reception envoye |
| EC-05 | On-call vet not configured -- emergency goes to all vets | P0 | EmergencyMessageReceivedEvent AllVets=true |
| EC-06 | Owner with 2 clinics sees 2 separate portals | P1 | Tokens distincts, conversations distinctes |

---

## 2. Tests E2E -- Playwright

### Emplacement : src/frontend/e2e/integration/

> MSW gere tous les mocks. Aucun page.route() autorise (regle CLAUDE.md). Donnees UAE : noms arabes/anglais, AED, timezone Asia/Dubai.

---

### 2.1 Navigation Inbox [P0]

Fichier : e2e/integration/messaging.spec.ts

| # | Test | data-testid requis |
|---|------|--------------------|
| IN-01 | Sidebar badge affiche le compteur non-lu, mis a jour via SSE simule | sidebar-messages-badge |
| IN-02 | Receptionniste voit uniquement AppointmentRequest et Administrative | conversation-list, conversation-item-{id} |
| IN-03 | Vet voit MedicalUrgency en premier avec badge CRITICAL rouge | conversation-item-{id}-priority-badge |
| IN-04 | Admin voit toutes les conversations, filtres fonctionnels | filter-status, filter-category, filter-date-range, apply-filters-btn |
| IN-05 | Assistant voit conversations non-medicales en lecture seule, sans bouton Reply | absence de conversation-reply-btn |
| IN-06 | Search par texte filtre la liste | search-conversations-input |

---

### 2.2 Envoi et reception de messages [P0]

| # | Test | data-testid requis |
|---|------|--------------------|
| MSG-01 | Vet ouvre conversation, panneau contexte patient visible (Luna, allergies) | patient-context-panel, patient-name, patient-allergies |
| MSG-02 | AI suggestions panel charge avec 1-3 suggestions | ai-suggestions-panel, ai-suggestion-{n} |
| MSG-03 | Clic sur suggestion pre-remplit le textarea | reply-textarea |
| MSG-04 | Vet modifie et envoie -- reply visible dans le fil | send-reply-btn, message-item-{id} |
| MSG-05 | Badge SSE mis a jour sans rechargement | sidebar-messages-badge |
| MSG-06 | Disclaimer AI affiche sur chaque suggestion (texte constant) | ai-disclaimer-text |
| MSG-07 | Resume AI en haut d une conversation > 5 messages, collapsible | conversation-summary-card, conversation-summary-toggle |
| MSG-08 | Note interne visuellement distincte (label Internal note) | internal-note-item, internal-note-label |

---

### 2.3 Alertes urgence [P0]

| # | Test | data-testid requis |
|---|------|--------------------|
| URG-01 | Notification browser push pour MedicalUrgency | emergency-notification-banner |
| URG-02 | Conversation MedicalUrgency en tete de liste avec badge CRITICAL rouge | priority-badge-critical |
| URG-03 | Escalation banner si message non vu apres 10 min | escalation-alert-banner |

---

### 2.4 Magic Link Owner Portal [P0]

Fichier : e2e/integration/owner-portal.spec.ts

| # | Test | data-testid requis |
|---|------|--------------------|
| OWN-01 | Portal affiche nom et logo de la clinique (Dubai Pet Care Clinic) | clinic-name-header, clinic-logo |
| OWN-02 | Ecran de consentement affiche avant le premier message | consent-checkbox, consent-submit-btn |
| OWN-03 | Selection de l animal (Luna) avant envoi | pet-selector, pet-option-{name} |
| OWN-04 | Selection categorie filtree selon animaux enregistres | category-selector |
| OWN-05 | Compteur de caracteres visible, Send desactive si > 2000 | char-counter, send-message-btn |
| OWN-06 | Upload 2 photos JPG, preview et bouton remove | photo-upload-input, photo-preview-{n}, remove-photo-{n}-btn |
| OWN-07 | Erreur si 4eme photo ajoutee | photo-limit-error |
| OWN-08 | Message envoye -- confirmation visible | send-success-toast |
| OWN-09 | Historique conversations affiche, notes internes absentes | conversation-list, conversation-thread |
| OWN-10 | Lien expire -- page d erreur This link has expired | expired-link-error |
| OWN-11 | Accuse de reception hors heures (message System visible) | system-message-item |
| OWN-12 | Bouton Download my messages, telechargement text/plain | download-messages-btn |

---

### 2.5 Responsive / RTL (AR) [P1]

Fichier : e2e/integration/messaging-i18n.spec.ts

| # | Test | Notes |
|---|------|-------|
| RTL-01 | Portal owner en arabe : layout RTL, textes traduits | Attribut dir=rtl sur html |
| RTL-02 | Staff inbox en arabe : labels traduits, layout RTL | locale=ar dans l URL |
| RTL-03 | Template bilingue : Admin cree template ContentEn + ContentAr | template-content-en, template-content-ar |
| RTL-04 | Suggestions AI en arabe pour message en arabe | ai-suggestion-0 contient Unicode arabe |
| MOB-01 | Owner portal responsive sur viewport 375x812 (iPhone) | viewport width 375, height 812 |
| MOB-02 | Staff inbox sur tablette 768px, panneau contexte collapse | context-panel-toggle |

---

### 2.6 Gestion templates et configuration Admin [P1]

Fichier : e2e/integration/messaging-admin.spec.ts

| # | Test | data-testid requis |
|---|------|--------------------|
| ADM-01 | Admin cree template bilingue, visible dans liste | template-name-input, template-content-en, template-content-ar, save-template-btn |
| ADM-02 | Admin configure horaires (Sun-Thu 08:00-20:00, Sat ferme) | hours-day-{0-6}, hours-open-{day}, hours-close-{day}, hours-closed-{day}-checkbox |
| ADM-03 | Admin dashboard triage stats : metriques visibles | stat-avg-response-time, stat-messages-by-category, stat-ai-accuracy |
| ADM-04 | Admin supprime un template, disparu de la liste | delete-template-{id}-btn |

---

## 3. Tests unitaires -- xUnit / NSubstitute

### Emplacement : tests/Vetolib.Tests.Unit/Messaging/

> Convention identique aux autres modules : NSubstitute pour les dependances, FluentAssertions, zero Testcontainers.

---

### 3.1 Domain -- Conversation State Machine [P0]

Fichier : Messaging/ConversationStateMachineTests.cs

| # | Test | Priorite |
|---|------|----------|
| SM-01 | Conversation.Create() retourne Result<Conversation> status=Open | P0 |
| SM-02 | Reply() sur Open passe a InProgress | P0 |
| SM-03 | Resolve() sur InProgress passe a Resolved | P0 |
| SM-04 | Close() sur Resolved passe a Closed | P0 |
| SM-05 | Reopen() sur Resolved passe a Open | P0 |
| SM-06 | Reopen() sur Closed retourne Result.Forbidden() | P0 |
| SM-07 | Message ajoute sur Closed retourne Result.Invalid() | P0 |
| SM-08 | Close() depuis InProgress par Admin : transition autorisee | P0 |

---

### 3.2 Domain -- Message Validation [P0]

Fichier : Messaging/MessageValidationTests.cs

| # | Test | Priorite |
|---|------|----------|
| MV-01 | Corps vide retourne Result.Invalid() | P0 |
| MV-02 | Corps > 2000 caracteres retourne Result.Invalid() | P0 |
| MV-03 | Corps exactement 2000 caracteres retourne Result.Success() | P0 |
| MV-04 | Plus de 3 pieces jointes retourne Result.Invalid() | P0 |
| MV-05 | Piece jointe > 5 MB retourne Result.Invalid() | P0 |
| MV-06 | Piece jointe MIME application/pdf retourne Result.Invalid() | P0 |
| MV-07 | Note interne creee par Receptionist retourne Result.Forbidden() | P0 |
| MV-08 | MessageSender.System ne peut etre positionne que par le systeme | P1 |

---

### 3.3 Domain -- Rate Limiting Owner [P0]

Fichier : Messaging/OwnerRateLimitTests.cs

| # | Test | Priorite |
|---|------|----------|
| RL-01 | 5eme message dans la journee accepte | P0 |
| RL-02 | 6eme message retourne Result.Invalid(daily limit) | P0 |
| RL-03 | Compteur reset a minuit (timezone Asia/Dubai) | P1 |

---

### 3.4 Domain -- Magic Link / OwnerPortalToken [P0]

Fichier : Messaging/OwnerPortalTokenTests.cs

| # | Test | Priorite |
|---|------|----------|
| TK-01 | Token valide (non expire) retourne Result.Success() | P0 |
| TK-02 | Token expire retourne Result.Unauthorized() | P0 |
| TK-03 | Token inconnu retourne Result.Unauthorized() | P0 |
| TK-04 | Token valide, ConsentAcceptedAt null, retourne Result.Forbidden() | P0 |
| TK-05 | Token valide avec consentement retourne Result.Success() | P0 |
| TK-06 | Validite 90 jours : 89 jours OK, 91 jours KO (boundary) | P1 |

---

### 3.5 Handlers -- Happy Path et Error Cases [P0]

Dossier : Messaging/Handlers/

| # | Handler | Happy path | Error path |
|---|---------|-----------|------------|
| H-01 | CreateConversationHandler | Conversation creee, TriageService appele, Result.Created | Owner sans token valide, Result.Unauthorized |
| H-02 | SendOwnerMessageHandler | Message ajoute, rate limit verifie | Rate limit depasse, Result.Invalid |
| H-03 | ReplyToConversationHandler | Message staff ajoute, OwnerMessageReplyEvent publie | Conversation Closed, Result.Conflict |
| H-04 | AddInternalNoteHandler | Note creee IsInternalNote=true | Receptionist, Result.Forbidden |
| H-05 | TransferConversationHandler | AssignedToUserId mis a jour | Conversation Closed, Result.Conflict |
| H-06 | MarkAsSpamHandler | IsSpam=true, absent inbox normal | Assistant, Result.Forbidden |
| H-07 | ConvertToAppointmentHandler | CreateAppointmentFromMessageCommand publie | Conversation sans patient, Result.Invalid |
| H-08 | CreateTemplateHandler | Template persiste EN + AR | ContentEn vide, Result.Invalid |
| H-09 | UpdateMessagingHoursHandler | 7 entrees MessagingHours persistees | Heures chevauchantes, Result.Invalid |
| H-10 | ListConversationsHandler | Receptionniste ne voit que non-medical | Vet voit medical + non-medical |
| H-11 | GetConversationHandler Portal | Notes internes filtrees | Token expire, Result.Unauthorized |
| H-12 | RecordConsentHandler | ConsentAcceptedAt + version persistes | Deja enregistre, idempotent |
| H-13 | ExportConversationsHandler | Stream text/plain | Owner sans conversations, fichier vide OK |

---

### 3.6 AI Classification Service [P0]

Fichier : Messaging/AiTriageServiceTests.cs

> Ces tests mockent IMessageTriageService via NSubstitute. Ils testent la logique d invocation et la gestion des resultats, pas l IA elle-meme.

| # | Test | Priorite |
|---|------|----------|
| AI-01 | Confidence < 0.7, IsTriageUncertain=true, route receptionist | P0 |
| AI-02 | Confidence >= 0.7, IsTriageUncertain=false, route selon category | P0 |
| AI-03 | Resultat MedicalUrgency, EmergencyMessageReceivedEvent publie | P0 |
| AI-04 | TriageService lance exception, handler retourne Result.Error (pas de throw) | P0 |
| AI-05 | Nombre de suggestions entre 1 et 3 | P0 |
| AI-06 | Aucune suggestion ne contient diagnose, prescribe, medication | P1 |
| AI-07 | Language detecte AR, suggestions en arabe | P1 |
| AI-08 | Biais emergency : incertitude entre types medicaux, resultat MedicalUrgency | P0 |

---

### 3.7 Business Hours Service [P1]

Fichier : Messaging/BusinessHoursServiceTests.cs

| # | Test | Priorite |
|---|------|----------|
| BH-01 | Dimanche 10:00 Asia/Dubai (dans heures configurees), IsOpen=true | P1 |
| BH-02 | Samedi IsClosed=true, 10:00, IsOpen=false | P1 |
| BH-03 | Vendredi 13:00 (apres 12:00 demi-journee), IsOpen=false | P1 |
| BH-04 | Jour ferie UAE Eid Al Fitr, IsOpen=false sauf override clinic | P1 |
| BH-05 | Heure limite exacte 20:00:00, IsOpen=false (fermeture exclusive) | P1 |
| BH-06 | SLA countdown ne compte que les heures ouvrees | P2 |

---

## 4. Tests d integration cross-module

### Emplacement : tests/Vetolib.Tests.Acceptance/StepDefinitions/Messaging/

> Reqnroll + Testcontainers PostgreSQL. Communication inter-modules via interfaces .Contracts uniquement.

---

### 4.1 Messaging vers Agenda [P0]

Feature : ConvertMessageToAppointment.feature

| # | Scenario | Verification |
|---|----------|--------------|
| MA-01 | Receptionniste convertit AppointmentRequest en RDV | CreateAppointmentFromMessageCommand consomme par Agenda, AppointmentDto cree avec champs pre-remplis |
| MA-02 | Vet cree RDV urgent depuis MedicalUrgency | type=Emergency, patient et reason extraits du message |
| MA-03 | On-call vet lookup via IOnCallVetReader.GetCurrentOnCallVetAsync | Vet on-call configure, notification uniquement a lui ; sinon broadcast |

---

### 4.2 Messaging vers MedicalRecords [P0]

Feature : MessageToMedicalRecord.feature

| # | Scenario | Verification |
|---|----------|--------------|
| MR-01 | Vet attache message au dossier medical | AddMessageToRecordCommand consomme, note creee avec texte + photo URLs |
| MR-02 | Patient context via IPatientReader.GetPatientContextAsync | last exam, prescriptions, allergies, vaccinations |
| MR-03 | Patient sans dossier, contexte vide, pas d erreur | Result.Success avec champs null/vides |

---

### 4.3 Messaging vers Notifications [P0]

Feature : MessagingNotifications.feature

| # | Scenario | Verification |
|---|----------|--------------|
| NOT-01 | Staff reply, OwnerMessageReplyEvent publie | Consumer Notifications recoit l event, email envoye |
| NOT-02 | MedicalUrgency, EmergencyMessageReceivedEvent publie | Push + email a tous les vets de la clinique |
| NOT-03 | Escalation 10 min, EmergencyEscalationEvent publie | Body contient URGENT -- unread emergency message |
| NOT-04 | Admin envoie magic link, SendMagicLinkEvent publie | Email contient URL /portal/{clinicSlug}?token=... |
| NOT-05 | Message hors heures non-urgent, MessageSender.System cree | Visible owner, pas de notification push |

---

### 4.4 Messaging vers Billing [P1]

Feature : ReceptionistBillingContext.feature

| # | Scenario | Verification |
|---|----------|--------------|
| BL-01 | Receptionniste voit factures impayees via IInvoiceReader.GetOutstandingForOwnerAsync | Contexte patient inclut solde en AED |
| BL-02 | Owner sans facture, outstanding vide, pas d erreur | Result.Success, outstanding=[] |

---

### 4.5 Messaging vers Auth (RBAC) [P0]

Feature : MessagingRBAC.feature

Couvre la matrice RBAC complete de l Appendix A de docs/MESSAGING-SPEC.md.

| # | Scenario | Verification |
|---|----------|--------------|
| RBAC-01 | Vet peut voir conversations medicales | HTTP 200 |
| RBAC-02 | Receptionist ne voit pas conversations medicales | Absentes de la reponse (filtre serveur) |
| RBAC-03 | Assistant ne peut pas reply | HTTP 403 |
| RBAC-04 | Receptionist ne peut pas ajouter note interne | HTTP 403 |
| RBAC-05 | Vet ne peut pas creer outbound conversation | HTTP 403 |
| RBAC-06 | Receptionist ne peut pas gerer templates | HTTP 403 |
| RBAC-07 | Admin peut effectuer toutes les actions | HTTP 200 sur chaque endpoint |
| RBAC-08 | IUserContext.Role utilise pour filtrage inbox | Test unitaire Handler avec NSubstitute IUserContext |

---

## 5. Tests de securite

### Emplacement : tests/Vetolib.Tests.Acceptance/Features/Messaging/MessagingSecurity.feature + enrichissement e2e/recette/security.spec.ts (existant)

---

### 5.1 Isolation tenant [P0]

| # | Test | Couche | Verification |
|---|------|--------|--------------|
| SEC-01 | Owner clinic A ne voit pas conversations de clinic B | BDD + Testcontainers | GET /api/v1/portal/conversations avec token clinic A, resultats clinic A uniquement |
| SEC-02 | Staff clinic A ne voit pas conversations de clinic B | BDD + Testcontainers | Global query filter ClinicId actif |
| SEC-03 | Toutes les entites Messaging implementent IMultiTenant | Unit | Conversation, Message, ResponseTemplate, OwnerPortalToken, MessagingHours |
| SEC-04 | IgnoreQueryFilters() absent du code metier Messaging | Grep | Aucune occurrence dans Modules/Messaging sauf seeds/migrations |

---

### 5.2 Magic Link expiration [P0]

| # | Test | Couche | Verification |
|---|------|--------|--------------|
| ML-01 | Token expire (> 90 jours), HTTP 401 | BDD | OwnerPortalToken.ExpiresAt dans le passe |
| ML-02 | Token inconnu, HTTP 401 | BDD | Token non trouve en base |
| ML-03 | Token d une autre clinique, HTTP 401 | BDD | ClinicId du token ne correspond pas a la clinique |
| ML-04 | Token valide mais owner sans acces au patient, HTTP 403 | BDD | Patient appartient a clinic A, token clinic B |
| ML-05 | Page d erreur expiration visible cote owner portal | E2E | data-testid=expired-link-error visible, formulaire absent |

---

### 5.3 Rate limiting messages [P0]

| # | Test | Couche | Verification |
|---|------|--------|--------------|
| RLM-01 | 5 messages/jour par owner -- le 6eme HTTP 429 | BDD | Header Retry-After present |
| RLM-02 | Rate limit isole par tenant | BDD | Owner clinic A non bloque par messages de owner clinic B |
| RLM-03 | Rate limit ASP.NET Core actif sur /api/v1/portal/conversations | Integration | Burst de requetes, 429 observable |

---

### 5.4 PDPL UAE Compliance [P1]

| # | Test | Couche | Verification |
|---|------|--------|--------------|
| PDPL-01 | Consentement obligatoire avant premier message | BDD | POST sans consentement HTTP 403 |
| PDPL-02 | Consentement enregistre avec timestamp et version | Unit | RecordConsentHandler persiste ConsentAcceptedAt + ConsentVersion |
| PDPL-03 | Export conversations disponible (droit d acces) | BDD | GET /api/v1/portal/export, text/plain |
| PDPL-04 | Export ne contient pas les notes internes | Unit | ExportConversationsHandler filtre IsInternalNote=true |
| PDPL-05 | Demande de suppression enregistree (droit d effacement) | BDD | POST /api/v1/portal/deletion-request, HTTP 202 |
| PDPL-06 | Notes internes non supprimees lors demande suppression owner | Unit | Messages owner supprimes, IsInternalNote=true conserves |
| PDPL-07 | Disclaimer AI est une constante string (non genere dynamiquement) | Unit | AiDisclaimer.Text est un const string, pas un appel IMessageTriageService |
| PDPL-08 | Encryption at rest verifiee en configuration | Architecture review | AES-256 PostgreSQL + stockage fichiers UAE region |

---

## 6. Fichiers a creer

### Backend -- Feature files BDD

    tests/Vetolib.Tests.Acceptance/Features/Messaging/
    -- MessageTriage.feature                (EXISTANT - corriger 3 enums + ajouter 5 scenarios)
    -- OwnerPortal.feature                  (NOUVEAU - 15 scenarios)
    -- ReceptionistInbox.feature            (NOUVEAU - 10 scenarios)
    -- VetInbox.feature                     (NOUVEAU - 12 scenarios)
    -- AdminMessaging.feature               (NOUVEAU - 9 scenarios)
    -- AssistantAccess.feature              (NOUVEAU - 4 scenarios)
    -- ConversationLifecycle.feature        (NOUVEAU - 6 scenarios)
    -- MessagingEdgeCases.feature           (NOUVEAU - 6 scenarios)
    -- ConvertMessageToAppointment.feature  (NOUVEAU cross-module Agenda)
    -- MessageToMedicalRecord.feature       (NOUVEAU cross-module MedicalRecords)
    -- MessagingNotifications.feature       (NOUVEAU cross-module Notifications)
    -- ReceptionistBillingContext.feature   (NOUVEAU cross-module Billing)
    -- MessagingRBAC.feature                (NOUVEAU - 8 scenarios)
    -- MessagingSecurity.feature            (NOUVEAU - 5 scenarios)



### Backend -- Step definitions

    tests/Vetolib.Tests.Acceptance/StepDefinitions/Messaging/
    -- OwnerPortalSteps.cs
    -- ReceptionistInboxSteps.cs
    -- VetInboxSteps.cs
    -- AdminMessagingSteps.cs
    -- AssistantAccessSteps.cs
    -- ConversationLifecycleSteps.cs
    -- MessagingEdgeCaseSteps.cs
    -- MessagingRBACSteps.cs



### Backend -- Tests unitaires

    tests/Vetolib.Tests.Unit/Messaging/
    -- ConversationStateMachineTests.cs
    -- MessageValidationTests.cs
    -- OwnerRateLimitTests.cs
    -- OwnerPortalTokenTests.cs
    -- BusinessHoursServiceTests.cs
    -- AiTriageServiceTests.cs
    -- Handlers/
       -- CreateConversationHandlerTests.cs
       -- SendOwnerMessageHandlerTests.cs
       -- ReplyToConversationHandlerTests.cs
       -- AddInternalNoteHandlerTests.cs
       -- TransferConversationHandlerTests.cs
       -- MarkAsSpamHandlerTests.cs
       -- ConvertToAppointmentHandlerTests.cs
       -- CreateTemplateHandlerTests.cs
       -- UpdateMessagingHoursHandlerTests.cs
       -- ListConversationsHandlerTests.cs
       -- GetConversationHandlerTests.cs
       -- RecordConsentHandlerTests.cs
       -- ExportConversationsHandlerTests.cs



### Frontend -- MSW Handlers

    src/frontend/src/mocks/handlers/
    -- messaging.ts      (Staff inbox, conversations, templates, stats, SSE)
    -- owner-portal.ts   (Portal owner : magic link, consent, pets, export)



### Frontend -- Tests E2E Playwright

    src/frontend/e2e/integration/
    -- messaging.spec.ts          (Staff inbox, navigation, SSE badge, urgences)
    -- owner-portal.spec.ts       (Portal owner complet)
    -- messaging-i18n.spec.ts     (RTL arabe, responsive mobile)
    -- messaging-admin.spec.ts    (Templates, business hours, stats)



---

## Resume des priorites

| Priorite | Nombre estime | Perimetre |
|----------|--------------|----------|
| **P0** | ~87 tests | State machine, triage routing, RBAC, isolation tenant, magic link, rate limit, PDPL consent, notifications urgence, cross-module Agenda/MedicalRecords/Notifications |
| **P1** | ~48 tests | ReplyAudit, templates bilingues, business hours, escalation, export PDPL, RTL/responsive, Billing context, suggestions AI en arabe |
| **P2** | ~10 tests | SLA countdown en heures ouvrees, analytics avances |

**Total estime** : ~145 tests a ecrire (hors scenarios deja existants dans MessageTriage.feature).

---

> Ce plan est aligne sur docs/MESSAGING-SPEC.md valide le 2026-03-10.
> Toute divergence entre ce plan et la spec finale doit etre escaladee via un fichier questions/ avant implementation.
