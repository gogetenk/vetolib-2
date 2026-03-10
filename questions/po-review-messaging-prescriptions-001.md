# PO Review -- Messaging & Prescriptions AI Features

**Date**: 2026-03-10
**Reviewer**: Product Owner
**Scope**: Messaging module (staff + owner portal), Prescriptions AI (drug catalog + interactions)

---

## 1. Messaging Staff -- RBAC Analysis

### Verdict: CONFORME

The RBAC matrix in the spec (Appendix A of MESSAGING-SPEC.md) is correctly implemented:

- **RECEPTIONIST**: sees only AppointmentRequest, Administrative, Other, Feedback. Cannot see medical conversations (MedicalUrgency, PostOperativeFollowUp, MedicalQuestion). Confirmed in the Gherkin scenarios and the `ListConversationsHandler` filtering by role routing.
- **VET**: sees medical conversations. Can reply, add internal notes, attach to medical record, create urgent appointment. All actions confirmed in `ConversationDetailPage.tsx` (`isVetOrAdmin` checks).
- **ADMIN**: global view, all actions, plus settings management (templates, hours, stats). Admin-only endpoints correctly use `RequireRole(AdminRole)` in `MessagingEndpoints.cs`.
- **ASSISTANT**: read-only on non-medical conversations. No reply button, no action buttons. Confirmed in `ConversationDetailPage.tsx` (`isAssistant` check hides ReplyComposer and ConversationActions).

**One gap identified**: The `SendReplyCommand` endpoint at line 63 of `MessagingEndpoints.cs` uses `.RequireAuthorization()` (any authenticated user) but does NOT explicitly exclude ASSISTANT. The handler itself should verify the role. If the handler does not check, an ASSISTANT could POST a reply via API even though the UI hides the button.

-> **Action needed**: Verify that `SendReplyHandler`, `AddInternalNoteHandler`, `TransferConversationHandler`, `RecategorizeConversationHandler`, and `MarkAsSpamHandler` all check `IUserContext.Role` and reject ASSISTANT requests with `Result.Forbidden()`. If not, create task `todo-back-messaging-rbac-hardening-001.md`.

---

## 2. Emergency Escalation (10 min) -- Business Realism

### Verdict: ACCEPTABLE FOR MVP, ADJUST IN V1.1

The 10-minute escalation timer for `MedicalUrgency` messages is **realistic and appropriate** for a veterinary clinic context:

- In human medicine, similar systems use 5-15 minute thresholds.
- Veterinary emergencies (poisoning, trauma, bloat) require faster response than human medicine in some cases, but the message is not a substitute for a phone call. The 10-minute window is for triaging the response, not for treating the animal.
- The spec correctly states "no further automatic escalation beyond step 2" -- the admin handles manual intervention after that. This is reasonable because over-alerting causes alert fatigue.

**Consideration for V1.1**: UAE clinics often have 1-2 vets on duty. If both are in surgery, 10 minutes may be too short. Consider making the escalation delay configurable per clinic (5-30 min range, default 10). This is NOT blocking for MVP.

---

## 3. Owner Rate Limit: 5 messages/day

### Verdict: SUFFICIENT FOR MVP, MONITOR POST-LAUNCH

5 messages per day per clinic is adequate for a messaging portal:

- An owner with a sick pet will typically send 1-2 messages and wait for a response.
- The limit prevents abuse (spam, anxiety-driven message floods).
- If owners consistently hit the limit, it means the clinic is not responding fast enough -- that is a clinic operational problem, not a rate limit problem.
- The error message "You have reached the daily message limit" with "try again tomorrow" is clear.

**Post-launch monitoring**: track how many owners hit the 5/day limit. If >10% of active owners hit it in the first month, consider increasing to 8-10. This is a configuration change, not an architecture change.

---

## 4. Owner Portal -- PDPL Consent

### Verdict: CONFORME

The consent flow is correctly implemented:

- **ConsentScreen.tsx**: checkbox + explicit acceptance required before first message. Records consent version ("1.0") and timestamp.
- **AcceptConsentCommand/Handler**: persists `ConsentAcceptedAt` and `ConsentVersion` on the `OwnerPortalToken` entity.
- **PortalLanding.tsx**: checks `sessionStorage` for consent status before allowing new message creation. First-time users are redirected to the consent page.
- **Export endpoint**: `GET /api/v1/portal/export` returns all conversations as text file (PDPL right of access).

**One gap**: The spec mentions "Right of deletion: the owner can request deletion. The clinic has 30 days to process." There is no deletion request endpoint in the portal API. This is acceptable for MVP (manual process via email), but should be a V1.1 feature.

---

## 5. Magic Link Authentication -- Security Assessment

### Verdict: ACCEPTABLE FOR MVP WITH CAVEATS

Magic link auth is a deliberate tradeoff between security and owner UX:

**Why it works for a vet portal**:
- Owners are not managing financial data (no payment, no invoices visible).
- The worst-case breach is someone reading conversation history about their pet -- sensitive but not financial.
- 90-day token validity is standard for magic links in healthcare portals (comparable to Doctolib patient portals).
- No password to forget = no support burden for clinics with non-tech-savvy pet owners.

**Security measures in place**:
- Token validated server-side via `MagicLinkEndpointFilter`.
- Token has expiration (`ExpiresAt`).
- Token is scoped to a single clinic (`ClinicId`).
- Portal endpoints are separate from staff endpoints (different auth mechanisms).

**Risks accepted**:
- Email compromise = portal access. Mitigated by the limited scope of visible data (no medical records, no internal notes, no financial data).
- Token forwarding (owner shares link). Acceptable -- they would only expose their own conversations.

**Not needed for MVP but recommended for V2**: rate limiting on magic link validation attempts, token revocation by clinic staff, IP-based anomaly detection.

---

## 6. Doctolib Feature Comparison

### Verdict: ON PAR FOR MVP

Comparing with Doctolib's patient messaging features:

| Feature | Doctolib | Vetolib MVP | Gap? |
|---------|----------|-------------|------|
| Messaging (text) | Yes | Yes | No |
| Photo attachments | Yes | Yes (3 max, 5MB each) | No |
| Conversation history | Yes | Yes | No |
| Read receipts | Yes | No | Minor (V2) |
| Document sharing (lab results) | Yes | Not explicitly | Yes -- see gap below |
| Prescription viewing | Yes | No | Yes -- out of scope per spec |
| Appointment booking from message | Yes | Yes (convert to appointment) | No |
| Push notifications | Yes | Yes (browser push for emergencies) | No |
| Email notifications | Yes | Yes | No |

**Gap: Document sharing from clinic to owner.** The spec allows staff to reply with text, but there is no explicit mechanism for attaching documents (lab results, X-ray reports, discharge summaries) from the clinic side to the owner. The `MessageAttachment` entity exists but is only used for owner-uploaded photos.

-> **Action needed**: Create question `questions/messaging-clinic-attachment-001.md` -- should staff be able to attach documents when replying? This is a high-value feature for vets (sharing lab results without phone calls). For MVP, text replies may suffice, but document sharing should be prioritized for V1.1.

---

## 7. Prescriptions AI -- Alert Types

### Verdict: THE 3 TYPES ARE THE MOST CRITICAL

The three alert types (species contraindication, drug-drug interaction, dosage out of range) are the highest-priority safety checks in veterinary prescribing:

1. **Species contraindication** (Critical): the single most dangerous error. Ibuprofen to cats, permethrin to cats, xylitol to dogs. These are the errors that kill animals. Highest priority, correctly classified as Critical.

2. **Drug-drug interaction** (Moderate): important for polypharmacy patients. Common in geriatric pets on multiple medications. Correctly classified as Moderate by default (can be Critical for specific pairs).

3. **Dosage out of range** (Info): valuable for weight-based dosing. Correctly classified as Info (warning, not blocking) because vets may intentionally dose above guidelines for therapeutic reasons.

**Missing for V2** (not blocking MVP):
- **Pregnancy/lactation contraindications**: relevant for breeding animals. Low frequency but important.
- **Age-based dosing adjustments**: pediatric and geriatric dosing differs. Nice to have.
- **Allergy/adverse reaction history**: patient-specific, requires a new field on Patient. High value but complex.
- **Renal/hepatic impairment dose adjustment**: relevant for geriatric pets. Complex, requires lab integration.

The 3 chosen types cover the most dangerous prescribing errors. The prioritization is correct.

---

## 8. Override with Mandatory Justification

### Verdict: CONFORME -- ESSENTIAL FOR CLINICAL SAFETY

The override mechanism is well-designed and standard practice in veterinary software:

- **Critical alerts require justification** (min 10 chars): correct. This prevents accidental overrides.
- **Moderate alerts allow proceeding without justification**: correct. Moderate interactions are common and often clinically acceptable.
- **Info alerts have no action required**: correct. These are informational only.
- **Audit trail**: override logged with vet ID, license number, timestamp, severity, justification. This is essential for malpractice protection and regulatory compliance.
- **RBAC**: only VET and ADMIN can override. ASSISTANT/RECEPTIONIST cannot create prescriptions at all. Correct.

The implementation in `MedicalRecordForm.tsx` correctly:
- Blocks submit when Critical alert present and no override confirmed (`isSubmitBlocked`)
- Shows `OverrideSection` component with justification textarea
- Shows confirmed override badge after justification

**One gap**: The `todo-back-prescriptions-override-001.md` task is still in `todo` status. The backend handler (`AddPrescriptionHandler.cs`) does NOT currently call `CheckInteractionsQuery` before saving. The frontend has the preflight UI but the backend does not enforce it. This means a direct API call could bypass all interaction checks.

-> **Action needed**: Prioritize `todo-back-prescriptions-override-001.md`. This is a safety-critical task. The backend MUST enforce interaction checks server-side, not rely on the frontend preflight alone.

---

## 9. Drug Catalog Size: 250 vs 500 Medications

### Verdict: SUFFICIENT FOR MVP

The spec mentions ~500 drugs in the seed (200 medications + 50 vaccines + others). For a UAE MVP:

- The top 200 medications cover 95%+ of daily prescriptions in a general practice veterinary clinic.
- 50 vaccines including camel-specific vaccines is important for UAE (camel care is a significant segment).
- ~100 interaction pairs and ~30 species contraindications cover the most dangerous combinations.
- Clinics can add custom drugs (clinic-scoped entries).

**The catalog grows over time** as clinics add their custom medications. The 250-500 seed is a starting point, not a ceiling.

For comparison, Plumb's Veterinary Drug Handbook covers ~600 drugs. The seed covers the essential core.

---

## 10. Frontend Components Review

### Conversation Detail Page: COMPLETE
- Message thread with chronological display
- AI suggestions panel (right panel on desktop, bottom on mobile)
- Patient context panel (role-filtered)
- Reply composer with note capability
- Action toolbar (transfer, status change, spam, convert to appointment)
- Triage uncertain badge
- `data-testid` on all interactive elements

### Admin Settings: COMPLETE
- Templates CRUD (TemplatesPage.tsx + TemplateFormDialog.tsx)
- Messaging hours configuration (MessagingHoursPage.tsx + DayHoursRow.tsx)
- Triage statistics dashboard (TriageStatsPage.tsx with Recharts: bar chart by category, line chart for volume, KPI cards for avg response time, AI accuracy, conversion rate)

### Owner Portal: COMPLETE
- Landing page with conversation list + new message button
- Consent screen with checkbox + terms
- New message form with pet selector, category selector, text area, photo upload
- Conversation view (PortalConversation.tsx)
- Export page
- Expired link handling
- Mobile-responsive layout

### Drug Selector: WELL DESIGNED
- Autocomplete with 300ms debounce (matches spec requirement)
- Free-text toggle for custom medications
- Selected drug summary
- Loading skeleton while searching
- No results state
- Keyboard navigation (Enter to select first result, Escape to close)
- `data-testid` on all elements

### Interaction Alerts Panel + Override Section: PRESENT
- InteractionAlertsPanel.tsx and OverrideSection.tsx exist
- DosageRangeIndicator.tsx exists
- AlternativeSuggestions.tsx exists
- Integrated into MedicalRecordForm.tsx with preflight API call

---

## 11. Gaps Summary and Actions

### Critical (safety/security)

| # | Gap | Action |
|---|-----|--------|
| 1 | Backend override enforcement missing (todo-back-prescriptions-override-001.md still in todo) | PRIORITIZE this task -- safety-critical |
| 2 | ASSISTANT role not explicitly rejected in messaging write handlers (API-level bypass possible) | Create task or verify handlers |

### Important (feature completeness)

| # | Gap | Action |
|---|-----|--------|
| 3 | No document attachment from clinic to owner (staff replies are text-only) | Study for V1.1 -- see question below |
| 4 | No owner deletion request endpoint (PDPL right of deletion) | V1.1 feature, manual process acceptable for MVP |
| 5 | SSE endpoint task not found (real-time updates for inbox badge) | Verify todo-back-messaging-sse-001.md exists and is planned |

### Nice to have (V2)

| # | Gap | Target |
|---|-----|--------|
| 6 | Read receipts for messages | V2 |
| 7 | Configurable escalation delay per clinic | V1.1 |
| 8 | Pregnancy/lactation contraindications in drug catalog | V2 |
| 9 | Owner-facing prescription history on portal | V2 |

---

## 12. Overall Assessment

**Messaging module**: The implementation is comprehensive, well-structured, and user-centered. The spec coverage is excellent -- all 6 feature files (OwnerPortal, ReceptionistInbox, VetInbox, AdminMessaging, MessageTriage, AssistantAccess) are present. The remaining tasks (AI integration, notifications, agenda integration, medical records integration) are correctly scoped as integration tasks that depend on the AI module being available. The frontend is complete for MVP.

**Prescriptions AI**: Phase 1 (catalog + prescription linking) and the frontend drug selector are done well. Phase 2 (interaction checking) has the frontend preflight UI done but the backend enforcement is still pending (todo-back-prescriptions-override-001.md). Phase 3 (stock integration) is correctly planned but not yet implemented. The 3 alert types are the right ones for MVP.

**Overall verdict**: Both features are well-aligned with the specs. The primary concern is the backend prescription override enforcement -- this is a patient safety feature that must be completed before any production deployment.

-> Escalade humain requise : non
