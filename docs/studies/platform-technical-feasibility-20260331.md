# Platform Technical Feasibility — Strategic Axes Assessment

> Date: 2026-03-31 | Author: Architect Agent | Status: DRAFT for PO review

---

## Scope

This document evaluates the technical feasibility and effort of three strategic axes before backlog dispatch:
1. Pet Owner Portal Enhancement
2. Public API / Open Standard (FHIR)
3. Multi-persona Architecture

Each axis is assessed on: existing assets, gaps, effort, risks, and blocking dependencies.

---

## 1. Pet Owner Portal Enhancement

### 1.1 What exists today

The portal is already **substantially built**. It is NOT a stub.

**Backend (Messaging module — `PortalEndpoints.cs`):**
- Magic link authentication via `OwnerPortalToken` + `MagicLinkEndpointFilter`
- GDPR/PDPL consent flow (`POST /consent`)
- Conversations: list, get by ID, create, send message
- Pet listing via cross-module `IPatientReader` contract
- Conversation export (PDPL right of access)
- Message categories (enum-based)
- Vet listing for booking (`ListClinicVeterinariansQuery` from Agenda.Contracts)

**Frontend (`/portal/[clinicSlug]/`):**
- Dedicated layout with `PortalLayout` component
- Landing page with conversation list, unread badges, status indicators
- New message flow with consent gate
- Conversation detail view
- Export page
- **Full booking wizard** (4-step: pet selection, consultation type, slot selection, confirmation)
- Booking management: list appointments, view detail, cancel, reschedule
- SSE endpoint exists for real-time updates (`SseEndpoints.cs`)

**Auth model:**
- Magic link tokens stored in `MessagingDbContext` (not Auth module)
- Token validation is cross-tenant by design (`IgnoreQueryFilters` on token lookup — correct and intentional)
- Token carries `OwnerId` + `ClinicId` — scoped to ONE clinic per token

### 1.2 What is missing for a "real" owner portal

| Feature | Exists? | Effort | Notes |
|---|---|---|---|
| Messaging (conversations) | YES | 0 | Fully functional |
| Booking appointments | YES | 0 | Full wizard + management |
| View pet profiles | PARTIAL | S | `listPortalPets()` returns basic info. Missing: photo, breed details, age display |
| Medical records consultation | NO | M | Requires new portal endpoint on MedicalRecords module. `GetPatientSummary` exists but is behind `RequireAuthorization()` (JWT only). Need a portal-scoped version |
| Vaccination history | NO | M | Data exists (`VACCINE:` prefix convention in Treatment field). Need a portal-safe query + frontend page |
| Push notifications / reminders | NO | L | Zero PWA infrastructure. `NotificationChannel.Push` enum exists but no implementation behind it |
| Prescription history (active meds) | NO | S | `GetPatientSummary` already computes active prescriptions (last 90 days). Needs portal-safe endpoint |
| Weight history / growth chart | NO | S | `WeightEntry` entity exists with `RecordedAt` + `WeightKg`. Frontend chart component needed |

### 1.3 Medical records for owners — detailed assessment

The `GetPatientSummaryHandler` already produces a comprehensive `PatientSummaryDto` including:
- Patient info (species, breed, sex, birth date, microchip, weight)
- Owner info
- Recent medical records (last 10, with diagnosis/treatment/vet/date)
- Active prescriptions (from last 90 days)
- Vaccinations (all records where Treatment starts with `VACCINE:`)
- Health alerts (records where Diagnosis starts with `ALLERGY:`)

**The data is ready.** The gap is purely in access control:
- Current endpoint: `GET /api/v1/patients/{id}/export/summary` requires `RequireAuthorization()` (JWT, vet/admin only)
- Needed: A new portal endpoint under `/api/v1/portal/patients/{id}/summary` that uses `MagicLinkEndpointFilter` and verifies the owner actually owns this patient

**Effort: Medium.** The handler logic can be reused (or a new query created that delegates to the same data access). The sensitive part is ensuring an owner can ONLY see their own patients. The `IPatientReader.GetPatientsByOwnerIdAsync` already provides this scoping.

### 1.4 PWA assessment

**Current state:** Zero PWA infrastructure. No `manifest.json`, no service worker (the existing `mockServiceWorker.js` is MSW for dev only), no `next-pwa` or workbox configuration.

**Effort to add PWA:**
- Add `manifest.json` with app name, icons, theme color, display: standalone
- Configure `next-pwa` (or equivalent) for service worker generation
- Add `meta` tags in root layout
- This gives installability + offline shell — **Small effort (1-2 days)**

**Push notifications via PWA:**
- Requires: Web Push API + VAPID keys + backend push subscription storage + notification dispatch
- The `NotificationChannel.Push` enum already exists in Notifications.Contracts, which means the architecture anticipated this
- Need: new entity `PushSubscription` (endpoint, p256dh, auth keys), new consumer `SendPushNotificationConsumer`, frontend subscription flow
- **Medium effort (3-5 days)** for basic push, not counting the UX for notification preferences

### 1.5 Verdict — Axis 1

| Aspect | Rating |
|---|---|
| Foundation | STRONG — portal is not a prototype, it's a functional product |
| Incremental cost | LOW to MEDIUM per feature |
| Biggest risk | Multi-clinic owner problem (see Section 4) |
| Recommended order | 1. Medical summary view, 2. Vaccination history, 3. PWA shell, 4. Push notifications |

---

## 2. Public API / Open Standard (FHIR)

### 2.1 FHIR R4 Patient-Animal compatibility

The FHIR R4 `Patient` resource has an official extension `patient-animal` (URL: `http://hl7.org/fhir/StructureDefinition/patient-animal`) with:
- `species` (CodeableConcept)
- `breed` (CodeableConcept)
- `genderStatus` (CodeableConcept — neutered/intact/unknown)

**Mapping from our Patient model:**

| Vetolib field | FHIR R4 field | Mapping complexity |
|---|---|---|
| `Patient.Id` | `Patient.id` | Direct |
| `Patient.Name` | `Patient.name[0].text` | Direct (FHIR Patient.name is for humans, but extension makes it work) |
| `Patient.Species` | `patient-animal.species` | Need mapping from our `Species` enum to SNOMED CT or custom CodeableConcept |
| `Patient.Breed` | `patient-animal.breed` | Free text to CodeableConcept — lossy unless we standardize breeds |
| `Patient.Sex` | `Patient.gender` + `patient-animal.genderStatus` | Need to split: Male/Female to gender, Neutered/Intact to genderStatus |
| `Patient.BirthDate` | `Patient.birthDate` | Direct (DateOnly to FHIR date) |
| `Patient.MicrochipNumber` | `Patient.identifier` (system: ISO 11784) | Direct — this is the universal key |
| `Owner.FirstName/LastName` | `Patient.contact[0].name` | FHIR models the owner as a contact on the animal Patient |
| `Owner.Phone` | `Patient.contact[0].telecom` | Direct |
| `Owner.Email` | `Patient.contact[0].telecom` | Direct |

**Verdict:** The mapping is feasible but not trivial. The main friction points are:
1. `Species` enum needs a SNOMED CT or local code system mapping
2. `Breed` is free text in our model — FHIR expects a CodeableConcept (can use `text` fallback)
3. `Sex` vs `genderStatus` distinction (neutered/intact) is not modeled in our `Sex` enum (which has Male/Female/Unknown)

### 2.2 Export endpoint (Patient + Medical Records + Prescriptions + Vaccinations)

**Data sources (all within MedicalRecords module):**
- `Patient` entity with owners
- `MedicalRecord` entities with prescriptions
- `WeightEntry` entities
- Vaccinations (convention: `Treatment` starts with `VACCINE:`)

**FHIR resources to generate:**
- `Patient` (with animal extension)
- `Encounter` (one per MedicalRecord)
- `Condition` (one per Diagnosis)
- `MedicationRequest` (one per Prescription)
- `Immunization` (one per vaccination record)
- `Observation` (weight entries)

**Complexity: HIGH.** This is not just serialization — it requires:
1. A FHIR serialization library (e.g., `Hl7.Fhir.R4` NuGet package — mature, well-maintained by Firely)
2. Resource builders for each entity type
3. A `Bundle` assembler (FHIR transaction or document bundle)
4. Proper code system mappings (SNOMED CT for species/breed, LOINC for observations)
5. Conformance/CapabilityStatement endpoint

**Effort estimate: 2-3 weeks** for a compliant export. The `Hl7.Fhir.R4` library handles serialization, but the mapping logic is substantial.

### 2.3 Import endpoint (deserialization + cross-tenant creation)

**This is where it gets architecturally dangerous.**

Importing a FHIR Bundle means:
1. Parse the incoming JSON (Firely SDK handles this)
2. Map FHIR resources back to Vetolib domain entities
3. Handle the cross-tenant problem: the imported animal may come from a DIFFERENT clinic

**Critical issue: multi-tenancy.** Our `MultiTenantDbContext` applies `WHERE ClinicId = @current` on ALL queries. An import creates data in the CURRENT clinic's tenant. This is semantically correct (the importing clinic owns their copy of the record), but it means:
- The imported patient gets a NEW `Id` in the target clinic
- The microchip number is the ONLY link to the original
- There is no cross-tenant reference system

**Effort: 3-4 weeks.** Mapping is harder in reverse (CodeableConcept to enum, handling unknown breeds/species, deduplication by microchip).

### 2.4 OAuth2 for partners

**Current auth stack:**
- JWT-based authentication for vet/admin users (Login + RefreshToken endpoints)
- Magic link tokens for portal owners (custom, not OAuth)
- No OAuth2 authorization server, no client credentials flow, no scopes

**What's needed for a public API:**
- An OAuth2 authorization server (e.g., OpenIddict, Duende IdentityServer, or Auth0/Azure AD B2C)
- Client credentials grant for machine-to-machine (partner integrations)
- Scope-based access control (e.g., `patient:read`, `records:read`, `records:write`)
- API key management UI for partners

**Effort: HIGH (3-4 weeks).** This is a foundational change. Recommendation: use an external provider (Auth0 or Azure AD B2C) rather than building an OAuth2 server in-house. Our Auth module currently handles its own JWT issuance (custom login handler with BCrypt) — adding a proper OAuth2 server on top is significant.

### 2.5 Rate limiting for public API

**Good news:** Rate limiting is already in place.
- `auth` policy: 10 req/min per IP
- `signup` policy: 3 req/h per IP
- `api` policy: 100 req/min per IP

For a public API, we would need:
- Per-client rate limiting (by API key / OAuth client ID, not just IP)
- Higher limits for paying partners, lower for free tier
- Separate rate limit policy (`api-public`) to not affect internal operations

**Effort: Small (2-3 days).** The ASP.NET Core rate limiter infrastructure is already wired. Just need a new policy with client-based partitioning.

### 2.6 Verdict — Axis 2

| Aspect | Rating |
|---|---|
| Export (FHIR) | FEASIBLE but HIGH effort — 2-3 weeks |
| Import (FHIR) | FEASIBLE but VERY HIGH effort + risk — 3-4 weeks |
| OAuth2 | HIGH effort if self-hosted, MEDIUM if using external provider |
| Rate limiting | Already partially done — LOW effort to extend |
| Overall | This is a 6-10 week project minimum. Recommend starting with READ-ONLY export, defer import. |

---

## 3. Multi-persona Architecture

### 3.1 Current frontend architecture

```
src/app/[locale]/
  (auth)/          <-- Route group: login, signup
  (dashboard)/     <-- Route group: vet/admin experience (sidebar, full features)
  portal/          <-- NOT a route group — regular folder with [clinicSlug]
```

**Next.js App Router already supports this perfectly.** Route groups `(auth)` and `(dashboard)` have separate layouts. The portal uses a regular folder (could be converted to a route group `(portal)` for symmetry, but it's not needed — it already has its own layout).

**Current layout separation:**
- `(dashboard)/layout.tsx` — sidebar navigation, header with clinic name, authenticated context
- `portal/[clinicSlug]/layout.tsx` — `PortalLayout` component, magic link token capture
- `(auth)/layout.tsx` — minimal layout for login/signup

**Verdict: The multi-persona architecture is already implemented.** Two distinct experiences exist with separate layouts, auth models, and navigation. This is not a future task — it is the current state.

### 3.2 Two types of auth

| Persona | Auth mechanism | Exists? |
|---|---|---|
| Vet/Admin/Staff | JWT (email + password, refresh token rotation) | YES — fully implemented |
| Pet Owner | Magic link (token in URL, stored in sessionStorage, sent via X-Portal-Token header) | YES — fully implemented |

**The backend enforces strict separation:**
- Staff endpoints: `RequireAuthorization()` (JWT validation)
- Portal endpoints: `MagicLinkEndpointFilter` (token validation)
- No cross-contamination possible — different middleware pipelines

### 3.3 What's missing for a polished multi-persona experience

| Gap | Effort | Notes |
|---|---|---|
| Portal navigation (sidebar/tabs) | S | Currently minimal — conversations + booking. Adding medical records / vaccinations requires nav updates |
| Owner profile page | S | No self-service profile editing for owners. Low priority |
| Owner notification preferences | M | Toggle: email vs push vs SMS. `NotificationChannel` enum exists, backend plumbing needed |
| Portal branding per clinic | S | Clinic logo/colors in portal. Currently generic PortalLayout |

### 3.4 Verdict — Axis 3

| Aspect | Rating |
|---|---|
| Architecture | ALREADY DONE — Next.js route groups + separate layouts + dual auth |
| Remaining work | Polish, not architecture. S-M features to add |
| Risk | LOW |

---

## 4. Technical Risks

### 4.1 Cross-module queries (owner views medical records)

**Situation:** An owner accessing their pet's medical record requires data from MedicalRecords module (patient, records, prescriptions, vaccinations, weight) — but the owner is authenticated via Messaging module (magic link).

**Current inter-module communication:**
- `IPatientReader` interface in `MedicalRecords.Contracts` — already used by Messaging module (`ListOwnerPetsHandler` delegates to `_patientReader.GetPatientsByOwnerIdAsync`)
- This pattern works and respects module isolation (Contracts only, no runtime reference)

**Performance:**
- `GetPatientSummaryHandler` executes 4 DB queries in sequence (patient + weight + records + vaccinations + alerts)
- All queries are within the same `MedicalRecordsDbContext` and benefit from the tenant filter
- For a portal use case (single patient view), this is acceptable. No N+1 problem.
- Output caching (`CacheOutput("Moderate2min")`) is already applied to `GetPatientDetail` — same pattern can be used for portal endpoint

**Risk: LOW.** The cross-module pattern is established and performant. Adding a portal-facing medical summary is an incremental extension, not an architectural change.

### 4.2 Multi-clinic owners (CRITICAL)

**The problem:** A pet owner may have animals in MULTIPLE clinics (e.g., general practice + specialist/referral). The current model has fundamental constraints:

1. **`OwnerPortalToken` is scoped to ONE clinic** (`ClinicId` + `OwnerId`). An owner with pets in 3 clinics gets 3 separate magic links, 3 separate portal sessions.

2. **`Owner` entity is per-tenant** (`IMultiTenant` with `ClinicId`). The same real person gets a separate `Owner` record in each clinic. There is NO cross-clinic owner identity.

3. **`MultiTenantDbContext` global query filter** enforces `WHERE ClinicId = @current` on every query. An owner portal session sees ONLY the data from the clinic that issued the magic link.

**Impact on strategic axes:**
- **Portal Enhancement:** An owner cannot see a unified view of their pets across clinics. Each clinic is a silo.
- **FHIR Import:** When importing a patient from another clinic, there is no way to link it to the same owner across tenants.
- **Shared medical history:** The entire value proposition of FHIR interop (referral history visible to the new clinic) requires cross-tenant reads, which the architecture explicitly prevents.

**Possible solutions (in order of invasiveness):**

**Option A: Keep siloed, unify in frontend only**
- Owner gets separate magic links per clinic
- Frontend shows a "clinic switcher" — owner can switch between clinic contexts
- No backend change needed, but UX is fragmented
- Effort: S | Risk: LOW | Value: LOW

**Option B: Introduce a global Owner identity**
- New `OwnerIdentity` entity in a new module (or Shared) that links `Owner` records across clinics by email + phone
- Magic link resolves to `OwnerIdentity`, which lists all clinics
- Portal queries use `IgnoreQueryFilters` with explicit `ClinicId IN (...)` — DANGEROUS, requires careful security review
- Effort: L-XL | Risk: HIGH (touches multi-tenancy core, FROZEN Shared) | Value: HIGH

**Option C: Microchip-based cross-reference (FHIR only)**
- When importing a FHIR Bundle, lookup by microchip number across tenants to detect "same animal, different clinic"
- Create a local copy of the patient (new tenant-scoped record) with a cross-reference link
- Does NOT require changing multi-tenancy — each clinic keeps its own copy
- Effort: M | Risk: MEDIUM | Value: MEDIUM (solves referral use case without touching core)

**Recommendation:** Start with Option A (zero backend risk) and design Option C for the FHIR interop track. Option B should only be considered if the product strategy requires a unified owner experience as a P0 feature, and it requires HUMAN ARBITRAGE since it touches frozen Shared.

### 4.3 Security — Owner can only see own animals

**Current enforcement:**
- `MagicLinkEndpointFilter` extracts `OwnerId` + `ClinicId` from the validated token
- `IPortalContext` exposes these to handlers
- `ListOwnerPetsHandler` calls `_patientReader.GetPatientsByOwnerIdAsync(ownerId, clinicId)` — scoped by design
- `ListOwnerConversationsQuery` includes `OwnerId` in the query — conversations are filtered by owner

**Gap:** For future endpoints (medical summary, vaccination history), every handler MUST include an ownership check: verify the requested `patientId` belongs to the authenticated `ownerId` before returning data. The `IPatientReader.GetPatientsByOwnerIdAsync` provides the ownership list — use it as the authorization gate.

**Risk: MEDIUM.** Not a flaw in the current system, but a mandatory pattern for every new portal endpoint. A single missed check = data leak. Recommend creating a reusable `IOwnerAuthorizationService` that validates patient-owner relationship.

### 4.4 RBAC for owner persona

**Current RBAC:**
- `UserRole` enum: Admin, Vet, Receptionist, Assistant
- NO `Owner` role — owners are not `User` entities at all
- Owners are authenticated via magic link, not JWT. They exist in `Owner` entity (MedicalRecords module) and `OwnerPortalToken` entity (Messaging module)

**This is a deliberate design choice, and it's correct.** Owners should NOT be mixed with staff users. The dual-auth model (JWT for staff, magic link for owners) provides clean separation. No RBAC change needed.

**However:** If owners ever need persistent accounts (e.g., to receive push notifications, manage preferences across sessions), a lightweight `OwnerAccount` entity would be needed. Magic links are stateless sessions — they don't support persistent preferences.

---

## 5. Effort Summary

| Work item | Effort | Dependencies | Priority |
|---|---|---|---|
| Portal: medical summary view | M (1 week) | New endpoint in MedicalRecords, reuse GetPatientSummary logic | HIGH |
| Portal: vaccination history page | S (3 days) | Same data source as summary, separate frontend page | HIGH |
| Portal: weight history chart | S (2 days) | WeightEntry data exists, need chart component | MEDIUM |
| PWA shell (installable + offline) | S (2 days) | next-pwa or manual SW config | MEDIUM |
| Push notifications (owner reminders) | M-L (1-2 weeks) | VAPID setup, PushSubscription entity, consumer, frontend subscription | MEDIUM |
| FHIR export (read-only) | L (2-3 weeks) | Hl7.Fhir.R4 NuGet, resource mappers, Bundle assembler | HIGH if interop is strategic |
| FHIR import | XL (3-4 weeks) | Cross-tenant microchip lookup, deduplication, conflict resolution | LOW priority initially |
| OAuth2 for partners | L (3-4 weeks) or M (1-2 weeks with Auth0) | External provider recommended | HIGH if public API is strategic |
| Public API rate limiting (per-client) | S (2-3 days) | Extend existing rate limiter | LOW (do when public API launches) |
| Multi-clinic owner UX (Option A) | S (3 days) | Frontend-only clinic switcher | MEDIUM |
| Cross-clinic owner identity (Option B) | XL (4+ weeks) | Touches FROZEN Shared — requires human arbitrage | DEFER |
| Owner authorization service | S (2 days) | Reusable pattern for all new portal endpoints | HIGH (do first) |

---

## 6. Recommendations

### Do first (before dispatching any backlog)
1. **Create `IOwnerAuthorizationService`** — reusable patient-ownership verification. Every new portal endpoint depends on this.
2. **Portal medical summary endpoint** — highest user value, lowest risk, data already exists.

### Do second (quick wins)
3. Vaccination history page (frontend + portal endpoint)
4. PWA shell (installable app, offline shell)

### Do third (medium effort, high strategic value)
5. FHIR export (read-only) — positions Vetolib as the interoperability leader
6. Push notifications for owners

### Defer (high risk or low urgency)
7. FHIR import — wait until export is validated with real partner clinics
8. OAuth2 for partners — wait until there are actual partners requesting API access
9. Cross-clinic owner identity — wait for clear product need, requires Shared modification

### Do NOT do
- Custom OAuth2 server in-house — use Auth0 or Azure AD B2C
- Mix owner accounts with staff `User` entities — keep the dual-auth model
- Attempt cross-tenant queries without a formal security review

---

## 7. Architecture Diagram — Target State

```
                    +------------------+
                    |   Next.js App    |
                    |   Router         |
                    +--------+---------+
                             |
              +--------------+--------------+
              |                             |
    +---------+----------+    +-------------+---------+
    | (dashboard) layout |    | portal/[slug] layout  |
    | JWT auth           |    | Magic link auth       |
    | Vet/Admin/Staff    |    | Pet Owner             |
    +--------------------+    +-----------------------+
              |                             |
              v                             v
    +--------------------+    +-----------------------+
    | /api/v1/*          |    | /api/v1/portal/*      |
    | RequireAuth (JWT)  |    | MagicLinkFilter       |
    +--------------------+    +-----------------------+
              |                             |
              v                             v
    +--------------------------------------------------+
    |          Module Layer (MediatR handlers)          |
    | +----------+ +--------+ +--------+ +----------+  |
    | |  Agenda  | |  Auth  | | MedRec | | Messaging|  |
    | +----------+ +--------+ +--------+ +----------+  |
    +--------------------------------------------------+
              |
              v
    +--------------------------------------------------+
    |     MultiTenantDbContext (ClinicId filter)        |
    |     PostgreSQL 16                                 |
    +--------------------------------------------------+
```

**New components needed (highlighted):**
- `IOwnerAuthorizationService` in MedicalRecords.Contracts
- Portal-scoped medical summary query in MedicalRecords module
- Portal-scoped vaccination history query in MedicalRecords module
- `PushSubscription` entity in Notifications module (future)
- FHIR resource mappers in a new `Vetolib.Interop` module (future)
