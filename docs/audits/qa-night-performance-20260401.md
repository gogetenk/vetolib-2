# Performance QA Audit -- 2026-04-01

Automated audit of all database queries across every module. Covers N+1 queries, missing `AsNoTracking`, missing indexes, unbounded queries, missing pagination, and OutputCache verification.

---

## 1. N+1 Queries

### P-01 [HIGH] GetPedigreeHandler -- recursive N+1 via IPatientReader

**File:** `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Queries/GetPedigree/GetPedigreeHandler.cs`

The handler recursively calls `_patientReader.GetPatientByIdAsync()` and `_context.PatientLineages.FirstOrDefaultAsync()` per generation node. For a 5-generation pedigree this triggers up to 2^5 = 31 individual queries (1 patient + 1 lineage per node).

**Fix:** Preload all lineages + patient data for the subtree in a single CTE or batch query, then walk the in-memory tree.

### P-02 [HIGH] GetDescendantsHandler -- recursive N+1 via IPatientReader

**File:** `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Queries/GetDescendants/GetDescendantsHandler.cs`

Same recursive pattern: `CollectDescendants` calls `_patientReader.GetPatientByIdAsync()` per child, then recurses to depth 5. In a large breeding program with 50+ descendants, this produces 50+ individual DB round-trips.

**Fix:** Same as P-01 -- batch-load all descendants in a single query, then assemble the tree in memory.

### P-03 [MEDIUM] GenerateHealthAlertsHandler -- IPatientAlertDataReader + nested loops

**File:** `src/backend/Modules/AI/Vetolib.AI/Application/Commands/GenerateHealthAlerts/GenerateHealthAlertsHandler.cs`

Loads all patients via `_patientDataReader.GetAllActivePatientsWithRecordsAsync()`, then iterates with `foreach (var patientDto in patients)` + `foreach (var rule in _rules)`. The inner loop calls `_dbContext.HealthAlerts.Add()` but the dedup check (`existingAlerts.Any(...)`) is done in-memory against a pre-loaded list -- this is acceptable. However, `GetAllActivePatientsWithRecordsAsync` itself may trigger N+1 depending on its implementation (not auditable from here since it is a contract interface).

**Fix:** Verify that `IPatientAlertDataReader.GetAllActivePatientsWithRecordsAsync` uses a single batched query with `.Include()` or projection. If it fetches records per patient, it is N+1.

### P-04 [LOW] SlotAvailableEventHandler -- foreach with no DB query inside

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Commands/NotifyWaitlist/SlotAvailableEventHandler.cs`

The `foreach` iterates over already-loaded `matchingEntries`, calling `entry.Notify()` (an in-memory domain method), then saves once. **Not an N+1** -- listed for completeness.

### P-05 [LOW] Logout/ChangePassword handlers -- foreach over tokens

**Files:**
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/Logout/LogoutHandler.cs`
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/ChangePassword/ChangePasswordHandler.cs`

Both load all active tokens into memory, then iterate with `token.Revoke()` in-memory, then single `SaveChangesAsync`. **Not an N+1** -- acceptable, bounded by per-user token count.

---

## 2. Missing AsNoTracking on Query Handlers

All query handlers (read-only) should use `.AsNoTracking()` to avoid change-tracker overhead. The following are missing it:

### P-06 [MEDIUM] PredictNextHeatHandler

**File:** `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Queries/PredictNextHeat/PredictNextHeatHandler.cs`

```csharp
var cycles = await _context.HeatCycles
    .Where(h => h.PatientId == query.PatientId)
    .OrderBy(h => h.StartDate)
    .ToListAsync(ct);   // <-- No AsNoTracking
```

### P-07 [MEDIUM] GetClassificationAccuracyHandler

**File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/GetClassificationAccuracy/GetClassificationAccuracyHandler.cs`

```csharp
var classifiedMessages = await _context.Messages
    .Where(m => m.ClassifiedUrgency != null)
    .Select(...)  // <-- Projection mitigates somewhat, but AsNoTracking is still best practice
    .ToListAsync(ct);
```

Note: the `.Select()` projection means EF won't track these anonymous objects, so the practical impact here is LOW. Still recommended for consistency.

### P-08 [MEDIUM] ListWaitlistEntriesHandler

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/ListWaitlistEntries/ListWaitlistEntriesHandler.cs`

```csharp
var entries = await _context.WaitlistEntries
    .OrderBy(e => e.CreatedAt)
    .Select(e => e.ToDto())  // projection mitigates, but entity may be tracked before Select
    .ToListAsync(ct);
```

### P-09 [MEDIUM] GetVisitFeedbackStatsHandler

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/GetVisitFeedbackStats/GetVisitFeedbackStatsHandler.cs`

Referenced in query handler list but confirmed in code -- uses `_context.VisitFeedbacks` without explicit `AsNoTracking()`. (The corresponding `ListVisitFeedbackHandler` correctly uses it.)

### P-10 [LOW] Handlers with .Select() projections

Several handlers skip `AsNoTracking()` but use `.Select()` projections which means EF Core doesn't track the result. These are **LOW priority** but should be standardized:
- `GetAppointmentCountHandler`
- `GetInvoiceCountHandler`
- `GetPatientCountHandler`
- `GetUnpaidInvoicesTotalHandler`

**Recommendation:** Add `.AsNoTracking()` to ALL query handlers as a convention, even when `.Select()` projections are used, for consistency and defense against future refactors.

---

## 3. Missing Indexes

### P-11 [HIGH] Prescription table -- no index on MedicalRecordId

**File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Infrastructure/PrescriptionConfiguration.cs`

The `Prescription` entity has a FK `MedicalRecordId` but no explicit index. EF Core auto-creates FK indexes by convention, but this should be verified. The `GetActivePrescriptionsForPatientHandler` does a JOIN on `MedicalRecordId` -- a missing index here would cause a seq scan on every prescription lookup.

Additionally, there is no index on `CreatedAt` which is used in the active prescription window filter (`p.CreatedAt >= cutoff`).

**Fix:** Add explicit composite index: `(ClinicId, MedicalRecordId)` and consider `(ClinicId, CreatedAt)`.

### P-12 [MEDIUM] StockItem table -- no index on Quantity/MinThreshold

**File:** `src/backend/Modules/Stock/Vetolib.Stock/Infrastructure/StockItemConfiguration.cs`

`GetStockAlertsHandler` queries `WHERE Quantity < MinThreshold` and `WHERE ExpiryDate <= threshold`. Neither column has an explicit index. For clinics with hundreds of stock items, this forces sequential scans.

**Fix:** Add index on `(ClinicId, Quantity)` and `(ClinicId, ExpiryDate)`.

### P-13 [MEDIUM] Conversation table -- no index on Status for filtered listing

**File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/ConversationConfiguration.cs`

`ListConversationsHandler` filters by `Status`, `Category`, `IsSpam`, and `AssignedToRole`. The existing indexes are `(ClinicId, CreatedAt)` and `(ClinicId, OwnerId)`. The status+category filter used for role-based inbox listing hits no index.

**Fix:** Add composite index `(ClinicId, Status, Category)` to support the primary listing query.

### P-14 [LOW] VisitFeedback table -- check for CreatedAt index

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/VisitFeedbackConfiguration.cs`

`ListVisitFeedbackHandler` orders by `CreatedAt` descending with pagination. Verify that an index on `(ClinicId, CreatedAt)` exists.

### P-15 [LOW] ConsultationType table -- no explicit index

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/ConsultationTypeConfiguration.cs`

`ListConsultationTypesHandler` filters by `IsActive` and sorts by `SortOrder, Name`. Low row count (typically < 50 per clinic) makes this LOW priority.

---

## 4. Unbounded Queries (ToListAsync without Take)

### P-16 [HIGH] ExportPatientFhirHandler -- loads ALL medical records + weight entries

**File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/ExportPatientFhir/ExportPatientFhirHandler.cs`

```csharp
var medicalRecords = await _context.MedicalRecords
    .AsNoTracking()
    .Where(r => r.PatientId == query.PatientId)
    .Include(r => r.Prescriptions)
    .ToListAsync(ct);   // <-- Unbounded

var weightEntries = await _context.WeightEntries
    .AsNoTracking()
    .Where(w => w.PatientId == query.PatientId)
    .ToListAsync(ct);   // <-- Unbounded
```

For a patient with 10+ years of records, this could load thousands of rows into memory at once.

**Fix:** Stream results or add reasonable limits. For FHIR exports, consider pagination in the FHIR bundle itself.

### P-17 [HIGH] ExportOwnerConversationsHandler -- loads ALL conversations + messages

**File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ExportOwnerConversations/ExportOwnerConversationsHandler.cs`

```csharp
var conversations = await _context.Conversations
    .Include(c => c.Messages.Where(m => !m.IsInternalNote))
    .Where(c => c.OwnerId == request.OwnerId)
    .ToListAsync(cancellationToken);   // <-- Unbounded
```

A prolific owner could have hundreds of conversations with thousands of messages. All loaded into memory.

**Fix:** Stream the export (chunked write) or add a date range filter.

### P-18 [HIGH] GetPatientSummaryHandler -- unbounded vaccination + allergy queries

**File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/GetPatientSummary/GetPatientSummaryHandler.cs`

```csharp
// Lines 73-82: loads ALL vaccination records for the patient
var vaccinations = await _context.MedicalRecords
    .Where(r => r.Treatment.StartsWith("VACCINE:"))
    .ToListAsync(ct);   // <-- Unbounded

// Lines 85-91: loads ALL allergy records
var healthAlerts = await _context.MedicalRecords
    .Where(r => r.Diagnosis.StartsWith("ALLERGY:"))
    .ToListAsync(ct);   // <-- Unbounded
```

While these are scoped to one patient, long-lived patients with extensive vaccination history could accumulate hundreds of records.

**Fix:** Add `.Take(100)` or similar reasonable limit; paginate if needed.

### P-19 [MEDIUM] GetSharedRecordHandler -- same unbounded pattern

**File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/GetSharedRecord/GetSharedRecordHandler.cs`

Same unbounded vaccination/allergy queries as GetPatientSummaryHandler (lines 94-114).

### P-20 [MEDIUM] GenerateHealthAlertsHandler -- loads ALL non-dismissed alerts

**File:** `src/backend/Modules/AI/Vetolib.AI/Application/Commands/GenerateHealthAlerts/GenerateHealthAlertsHandler.cs`

```csharp
var existingAlerts = await _dbContext.HealthAlerts
    .Where(a => a.Status != HealthAlertStatus.Dismissed)
    .ToListAsync(cancellationToken);   // <-- Unbounded
```

Over time, active alerts can accumulate. Also, no `AsNoTracking()` on a read for dedup purposes.

**Fix:** Use `AsNoTracking()` and consider projecting only `(PatientId, RuleId)` tuples for dedup.

### P-21 [MEDIUM] ListWaitlistEntriesHandler -- no pagination

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/ListWaitlistEntries/ListWaitlistEntriesHandler.cs`

```csharp
var entries = await _context.WaitlistEntries
    .OrderBy(e => e.CreatedAt)
    .Select(e => e.ToDto())
    .ToListAsync(ct);   // <-- No Take, no pagination
```

### P-22 [MEDIUM] ListOwnerConversationsHandler -- no pagination

**File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListOwnerConversations/ListOwnerConversationsHandler.cs`

```csharp
var conversations = await _context.Conversations
    .Where(c => c.OwnerId == request.OwnerId)
    .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
    .Select(c => c.ToDto())
    .ToListAsync(cancellationToken);   // <-- Unbounded
```

### P-23 [LOW] ListConsultationTypesHandler -- unbounded but naturally bounded

Loads all active consultation types. Naturally bounded (< 50 per clinic). LOW risk.

### P-24 [LOW] GetStockAlertsHandler -- unbounded lowStock/expiring queries

Both queries lack `.Take()`. Bounded by actual stock item count per clinic (typically < 500). LOW risk.

---

## 5. Missing Pagination on List Endpoints

### P-25 [HIGH] GET /api/v1/patients/{patientId}/heat-cycles -- no pagination

**File:** `src/backend/Modules/Breeding/Vetolib.Breeding/Api/HeatCycleEndpoints.cs`

Returns all heat cycles for a patient, no page/pageSize parameters.

### P-26 [HIGH] GET /api/v1/waitlist -- no pagination

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Api/WaitlistEndpoints.cs`

Returns all waitlist entries with no limit.

### P-27 [HIGH] GET /api/v1/messaging/portal/{ownerId}/conversations -- no pagination

Returns all conversations for an owner. Combined with P-22, this is a potential memory bomb for active owners.

### P-28 [MEDIUM] GET /api/v1/stock/alerts -- no pagination

Returns all low-stock and expiring items. Naturally bounded but should still have a limit.

### P-29 [LOW] GET /api/v1/consultation-types -- no pagination

Returns all active consultation types. Naturally bounded (< 50).

**Endpoints WITH correct pagination (verified):**
- `GET /api/v1/appointments` -- pageNumber/pageSize
- `GET /api/v1/stock` -- pageNumber/pageSize
- `GET /api/v1/messaging/conversations` -- page/pageSize
- `GET /api/v1/patients` -- page/pageSize
- `GET /api/v1/invoices` -- (via ListInvoicesHandler with Skip/Take)
- `GET /api/v1/visit-feedback` -- pageNumber/pageSize

---

## 6. OutputCache Verification

### Registered Policies (in Program.cs)

| Policy | TTL | VaryBy |
|---|---|---|
| `Dashboard1min` | 1 min | Query `*` + `Authorization` header |
| `Moderate2min` | 2 min | Query `*` + `Authorization` header |

### Endpoints Using CacheOutput

| Endpoint | Policy | Status |
|---|---|---|
| Dashboard: metrics, today-appointments, patients-by-species, etc. (7 endpoints) | `Dashboard1min` | OK -- varies by Auth |
| `GET /api/v1/patients` (list) | Inline `SetVaryByQuery(...)` + `Tag("patients")` | OK -- 30s, query-specific |
| `GET /api/v1/patients/{id}` | `Moderate2min` | OK |
| `GET /api/v1/patients/{id}/detail` | `Moderate2min` | OK |

### Cache Invalidation

| Mutating Handler | Evicts |
|---|---|
| `EditAppointmentHandler` | Injects `IOutputCacheStore?` | Should verify tag-based eviction |
| `ImportPatientsHandler` | Evicts `"patients"` + `"dashboard"` tags | OK |

### P-30 [MEDIUM] Missing cache on frequently-hit read endpoints

The following read-heavy endpoints have no OutputCache:
- `GET /api/v1/consultation-types` -- rarely changes, high read frequency
- `GET /api/v1/messaging/settings/hours` -- rarely changes
- `GET /api/v1/stock/alerts` -- could benefit from 30-60s cache
- `GET /api/v1/preferences/working-hours` -- rarely changes

**Fix:** Add `CacheOutput("Moderate2min")` or similar to these endpoints.

### P-31 [LOW] No eviction on appointment status change

`UpdateAppointmentStatusHandler` changes appointment status but does not evict the `Dashboard1min` cache. The dashboard will show stale data for up to 1 minute after a check-in or completion. Acceptable for 1-min TTL but worth noting.

---

## Summary Table

| ID | Category | Severity | Module | Description |
|---|---|---|---|---|
| P-01 | N+1 | HIGH | Breeding | GetPedigreeHandler recursive N+1 |
| P-02 | N+1 | HIGH | Breeding | GetDescendantsHandler recursive N+1 |
| P-03 | N+1 | MEDIUM | AI | GenerateHealthAlerts -- verify IPatientAlertDataReader |
| P-06 | AsNoTracking | MEDIUM | Breeding | PredictNextHeatHandler |
| P-07 | AsNoTracking | LOW | Messaging | GetClassificationAccuracyHandler (mitigated by .Select) |
| P-08 | AsNoTracking | MEDIUM | Agenda | ListWaitlistEntriesHandler |
| P-09 | AsNoTracking | MEDIUM | Agenda | GetVisitFeedbackStatsHandler |
| P-11 | Index | HIGH | MedicalRecords | Prescription: no index on CreatedAt |
| P-12 | Index | MEDIUM | Stock | StockItem: no index on Quantity/ExpiryDate |
| P-13 | Index | MEDIUM | Messaging | Conversation: no index on Status+Category |
| P-16 | Unbounded | HIGH | MedicalRecords | ExportPatientFhir loads ALL records |
| P-17 | Unbounded | HIGH | Messaging | ExportOwnerConversations loads ALL |
| P-18 | Unbounded | HIGH | MedicalRecords | GetPatientSummary unbounded vaccinations |
| P-19 | Unbounded | MEDIUM | MedicalRecords | GetSharedRecord same pattern |
| P-20 | Unbounded | MEDIUM | AI | GenerateHealthAlerts loads all active alerts |
| P-21 | Unbounded | MEDIUM | Agenda | ListWaitlistEntries no pagination |
| P-22 | Unbounded | MEDIUM | Messaging | ListOwnerConversations no pagination |
| P-25 | Pagination | HIGH | Breeding | Heat cycles endpoint no pagination |
| P-26 | Pagination | HIGH | Agenda | Waitlist endpoint no pagination |
| P-27 | Pagination | HIGH | Messaging | Owner portal conversations no pagination |
| P-30 | Cache | MEDIUM | Multiple | Missing cache on frequently-read endpoints |

### Counts by Severity

- **HIGH:** 9 findings
- **MEDIUM:** 11 findings
- **LOW:** ~8 findings (minor, best-practice items)

### Top 3 Priority Fixes

1. **P-01 + P-02** (Breeding N+1): Convert recursive GetPedigree/GetDescendants to batch-loaded queries. Could cause visible latency on pedigree pages.
2. **P-16 + P-17** (Unbounded exports): Add streaming or limits to FHIR export and conversation export. Risk of OOM on large datasets.
3. **P-11** (Prescription index): Add composite index on Prescription table for MedicalRecordId + CreatedAt. Affects active prescription lookups on every patient detail view.
