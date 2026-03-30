# Performance Audit - 2026-03-29

Audit of the Vetolib backend codebase. Report-only -- no fixes applied.

Severity scale: **CRITICAL** (production incident risk), **HIGH** (measurable perf hit), **MEDIUM** (latent risk, scales badly), **LOW** (minor optimization opportunity).

---

## 1. N+1 Queries

**No classic N+1 (foreach + await DB) found.**

The only `await foreach` is in `Messaging/SseEndpoints.cs` (SSE streaming from a Channel), which is correct and not a DB loop.

All handlers use single LINQ queries with `.Include()` where needed. No foreach-then-query pattern detected.

**Verdict: CLEAN**

---

## 2. Missing AsNoTracking on Read-Only Queries

Most query handlers correctly use `.AsNoTracking()`. However, two read-only handlers are missing it:

### Finding P-01: GetHealthAlertsHandler (AI module) -- missing AsNoTracking
- **File:** `src/backend/Modules/AI/Vetolib.AI/Application/Queries/GetHealthAlerts/GetHealthAlertsHandler.cs`
- **Severity:** LOW
- The query uses `_context.HealthAlerts.Where(...)` without `.AsNoTracking()`. The entities are projected via `MapToDto()` but EF still tracks them.
- **Impact:** Minor memory overhead per request; negligible until alert volume grows.

### Finding P-02: GetPatientHealthAlertsHandler (AI module) -- missing AsNoTracking
- **File:** `src/backend/Modules/AI/Vetolib.AI/Application/Queries/GetPatientHealthAlerts/GetPatientHealthAlertsHandler.cs`
- **Severity:** LOW
- Same issue as P-01.

### Finding P-03: ListTemplatesHandler (Messaging module) -- missing AsNoTracking
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListTemplates/ListTemplatesHandler.cs`
- The query `_context.ResponseTemplates.AsNoTracking()` IS used (line 20). CLEAN.

**Overall: Good discipline. Only the AI module has 2 minor misses.**

---

## 3. Missing Indexes

### Finding P-04: medical_records table -- no index on PatientId
- **File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Infrastructure/MedicalRecordConfiguration.cs`
- **Severity:** HIGH
- `MedicalRecord.PatientId` is used in WHERE clauses by: `ListMedicalRecordsHandler`, `GetPatientDetailHandler`, `GetPatientContextAsync` (PatientReader), and `GetActivePrescriptionsForPatientHandler`.
- No `HasIndex` is defined on `PatientId` (or `{ClinicId, PatientId}`).
- **Impact:** Full table scan on `medical_records` for every patient detail view and prescription lookup. Degrades linearly with record volume.

### Finding P-05: Conversations table -- no index on OwnerId
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/ConversationConfiguration.cs`
- **Severity:** MEDIUM
- `ListOwnerConversationsHandler` and `ExportOwnerConversationsHandler` filter by `c.OwnerId`. Only an index on `{ClinicId, CreatedAt}` exists.
- **Impact:** Slow queries for owner portal conversation lists as conversation volume grows.

### Finding P-06: Conversations table -- no index on Status or Category
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/ConversationConfiguration.cs`
- **Severity:** LOW
- `ListConversationsHandler` filters by `Status`, `Category`, and `IsSpam`. Only `{ClinicId, CreatedAt}` is indexed.
- The multi-tenant global filter on ClinicId partially helps, but compound index with Status/Category would improve clinic inbox queries.

### Finding P-07: Invoices table -- no index on Status or CreatedAt
- **File:** `src/backend/Modules/Billing/Vetolib.Billing/Infrastructure/InvoiceConfiguration.cs`
- **Severity:** MEDIUM
- `ListInvoicesHandler` orders by `CreatedAt`. `GetRevenueByMonthHandler` filters by `Status == Paid && CreatedAt >= cutoff`. `GetUnpaidInvoicesTotalHandler` likely filters by Status.
- Only `{ClinicId, InvoiceNumber}` (unique) is indexed.
- **Impact:** Revenue analytics and invoice listing degrade with invoice count. Each dashboard load triggers these queries.

### Finding P-08: Prescriptions table -- no apparent index
- The `PrescriptionConfiguration` file is not adding explicit indexes beyond the FK `MedicalRecordId`. `GetActivePrescriptionsForPatientHandler` joins Prescriptions with MedicalRecords by `MedicalRecordId` and filters by `CreatedAt`.
- **Severity:** MEDIUM (depends on prescription volume)

---

## 4. Large Payloads / Missing Pagination

### Finding P-09: ListInvoicesHandler -- unbounded query, no pagination
- **File:** `src/backend/Modules/Billing/Vetolib.Billing/Application/Queries/ListInvoices/ListInvoicesHandler.cs`
- **Severity:** HIGH
- Returns ALL invoices for the clinic with `.Include(i => i.Items)`, ordered by `CreatedAt DESC`, with no `Skip/Take`.
- **Impact:** A clinic with 10,000+ invoices will load all of them + all line items into memory on every call. Response payload grows without bound.

### Finding P-10: ListStockItemsHandler -- unbounded query, no pagination
- **File:** `src/backend/Modules/Stock/Vetolib.Stock/Application/Queries/ListStockItems/ListStockItemsHandler.cs`
- **Severity:** MEDIUM
- Returns all stock items matching filters (category, low stock, expiring soon) with no `Skip/Take`.
- Mitigated by filters, but "show all items" returns everything.

### Finding P-11: GetTodayAppointmentsHandler -- returns all appointments for today
- **File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/GetTodayAppointments/GetTodayAppointmentsHandler.cs`
- **Severity:** LOW
- Bounded by "today" filter; a single clinic rarely has more than ~100 daily appointments. Acceptable.

### Finding P-12: ListOwnerConversationsHandler -- unbounded query for owner portal
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListOwnerConversations/ListOwnerConversationsHandler.cs`
- **Severity:** MEDIUM
- Loads all conversations for an owner with no pagination. An active pet owner with years of history could accumulate hundreds of conversations.

### Finding P-13: ExportOwnerConversationsHandler -- loads all conversations with all messages
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ExportOwnerConversations/ExportOwnerConversationsHandler.cs`
- **Severity:** MEDIUM
- `.Include(c => c.Messages)` on all owner conversations. Entire message history loaded into memory for text export.
- **Impact:** Memory spike for owners with long conversation histories. Consider streaming or chunked export.

### Finding P-14: GetLittersByMotherHandler -- unbounded with Include
- **File:** `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Queries/GetLittersByMother/GetLittersByMotherHandler.cs`
- **Severity:** LOW
- Loads all litters + offspring. Biologically bounded (a mother has few litters). Acceptable.

---

## 5. Missing Caching

### Finding P-15: ConsultationTypes -- no cache on rarely-changing data
- **File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Queries/ListConsultationTypes/ListConsultationTypesHandler.cs`
- **Severity:** MEDIUM
- Consultation types rarely change but are queried on every appointment creation form load. No output cache or memory cache.
- **Impact:** Unnecessary DB round-trip on high-traffic page.

### Finding P-16: Drug catalog search -- no cache
- **File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/SearchDrugCatalog/SearchDrugCatalogHandler.cs`
- **Severity:** LOW
- The global drug catalog is seeded and rarely updated. Repeated searches hit the DB every time. Would benefit from in-memory cache or output cache with short TTL.

### Finding P-17: Dashboard stats -- good caching, but sequential queries
- **File:** `src/backend/Vetolib.Api/Dashboard/DashboardEndpoints.cs`
- **Severity:** MEDIUM
- The `GetStats` endpoint sends 3 MediatR queries **sequentially** (await, await, await). Each hits a different module DB context.
- `GetAnalytics` sends 3 queries sequentially as well.
- The `CacheOutput("Dashboard1min")` mitigates frequency, but on cache miss, the response time is the **sum** of all 3 query latencies.
- **Fix:** Use `Task.WhenAll` to parallelize the independent queries.

---

## 6. Async All the Way / Sync-over-Async

**No sync-over-async patterns found.** No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` detected in production code. All DB calls use async EF Core methods.

**Verdict: CLEAN**

---

## 7. EF Core Specific Issues

### Finding P-18: Cartesian explosion risk -- SearchDrugCatalogHandler triple Include
- **File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/SearchDrugCatalog/SearchDrugCatalogHandler.cs`
- **Severity:** HIGH
- `.Include(d => d.SpeciesContraindications).Include(d => d.Interactions).Include(d => d.DosageGuidelines)` on a single query.
- EF Core generates a single SQL with JOINs across 3 child collections. This produces a Cartesian product: if a drug has 5 contraindications, 3 interactions, and 4 dosage guidelines, the result set is 5x3x4 = 60 rows per drug.
- **Impact:** With 20 drugs returned (default limit), potentially thousands of rows transferred. `.AsSplitQuery()` would fix this.

### Finding P-19: Cartesian explosion risk -- GetPatientContextAsync
- **File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Infrastructure/PatientReader.cs` (line 55-58)
- **Severity:** HIGH
- `.Include(p => p.MedicalRecords).ThenInclude(r => r.Prescriptions)` loads ALL medical records and ALL prescriptions for a patient.
- No date filter, no `.Take()`. A patient with 5 years of records could have hundreds of records, each with multiple prescriptions.
- This query is called from `GetConversationByIdHandler` when `includeFullMedicalContext = true`.
- **Impact:** Unbounded data load + Cartesian product between MedicalRecords and Prescriptions.

### Finding P-20: ListConversationsHandler -- Include(Messages) on paginated query
- **File:** `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListConversations/ListConversationsHandler.cs` (line 99)
- **Severity:** MEDIUM
- `.Include(c => c.Messages)` loads ALL messages for each conversation in the page. If the list shows 20 conversations with 50 messages each, that is 1,000+ message rows loaded just to render a list.
- The `ToDto()` likely only needs the last message or a message count.
- **Impact:** Over-fetching on every inbox page load.

### Finding P-21: AggregateEReportingDataHandler -- full materialization of invoices + items
- **File:** `src/backend/Modules/Billing/Vetolib.Billing/Application/Queries/AggregateEReportingData/AggregateEReportingDataHandler.cs`
- **Severity:** MEDIUM
- Loads all B2C invoices with `.Include(i => i.Items).ToListAsync()` then aggregates in-memory.
- The GroupBy + Sum could be pushed to the database via a LINQ query without materializing the full entity graph.
- Same pattern in `SubmitEReportingHandler`.
- **Impact:** Memory-heavy for large invoice volumes.

---

## 8. Startup Performance

### Finding P-22: Sequential migration of 7 DbContexts at startup
- **File:** `src/backend/Vetolib.Api/DbInitializer.cs`
- **Severity:** MEDIUM
- `MigrateAllAsync` runs 7 migrations sequentially: Auth, Agenda, MedicalRecords, Billing, Audit, Notifications, Breeding.
- Each `MigrateAsync()` opens a connection, acquires a lock, checks migration history, and applies pending migrations.
- **Impact:** Cold start takes 7 x (connection + lock + check) time. In production with no pending migrations, this is ~7 round-trips. With pending migrations, it is much worse.
- **Note:** Parallelizing is risky (shared Postgres lock). Consider running migrations out-of-band (CI/CD pipeline or init container) instead of at app startup.

### Finding P-23: BCrypt.HashPassword called synchronously during seed
- **File:** `src/backend/Vetolib.Api/DbInitializer.cs` (lines 119, 126)
- **Severity:** LOW
- BCrypt hashing is CPU-intensive (~100ms per hash). Two hashes = ~200ms added to startup. Only runs once (idempotent seed), so acceptable.

### Finding P-24: DrugCatalogSeedData called at every startup
- **File:** `src/backend/Vetolib.Api/Program.cs` (line 300)
- **Severity:** LOW
- `SeedDrugCatalogAsync` is called on every startup. If idempotent (checks before inserting), impact is one SELECT query. If it does bulk operations regardless, this adds startup latency.

### Finding P-25: DbContext pooling not used for multi-tenant contexts
- **File:** `src/backend/Vetolib.Api/Program.cs` (lines 101-104, comment)
- **Severity:** LOW
- The code explicitly avoids `AddDbContextPool` because multi-tenant `IClinicContext` is scoped. This is architecturally correct but means every request creates a new DbContext instance.
- **Impact:** Slightly higher GC pressure. Acceptable trade-off for multi-tenancy.

---

## 9. Memory Issues

### Finding P-26: No obvious memory leaks detected

- No event handler subscriptions without cleanup.
- No static collections growing unbounded.
- MassTransit consumers are correctly registered via DI (transient/scoped).
- The `IMemoryCache` in Preferences module has a 5-minute TTL with explicit eviction on change. No leak risk.
- SSE endpoint in `SseEndpoints.cs` reads from a Channel with `CancellationToken` -- will clean up when client disconnects.

### Finding P-27: PatientReader.GetPatientContextAsync -- large object graph in memory
- **File:** `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Infrastructure/PatientReader.cs`
- **Severity:** MEDIUM
- Already covered in P-19. This loads an unbounded entity graph (all medical records + prescriptions) into memory. Not a "leak" but a memory spike risk per request.

---

## Summary Table

| ID | Finding | Module | Severity | Category |
|----|---------|--------|----------|----------|
| P-01 | Missing AsNoTracking in GetHealthAlertsHandler | AI | LOW | Tracking |
| P-02 | Missing AsNoTracking in GetPatientHealthAlertsHandler | AI | LOW | Tracking |
| P-04 | No index on medical_records.PatientId | MedicalRecords | HIGH | Index |
| P-05 | No index on conversations.OwnerId | Messaging | MEDIUM | Index |
| P-06 | No index on conversations.Status/Category | Messaging | LOW | Index |
| P-07 | No index on invoices.Status/CreatedAt | Billing | MEDIUM | Index |
| P-08 | No index on prescriptions beyond FK | MedicalRecords | MEDIUM | Index |
| P-09 | ListInvoices -- unbounded, no pagination | Billing | HIGH | Pagination |
| P-10 | ListStockItems -- no pagination | Stock | MEDIUM | Pagination |
| P-12 | ListOwnerConversations -- no pagination | Messaging | MEDIUM | Pagination |
| P-13 | ExportOwnerConversations -- all messages in memory | Messaging | MEDIUM | Payload |
| P-15 | ConsultationTypes -- no cache | Agenda | MEDIUM | Caching |
| P-16 | Drug catalog search -- no cache | MedicalRecords | LOW | Caching |
| P-17 | Dashboard sequential queries (should parallelize) | Api | MEDIUM | Latency |
| P-18 | Cartesian explosion -- triple Include on drug search | MedicalRecords | HIGH | EF Core |
| P-19 | Unbounded Include on patient medical context | MedicalRecords | HIGH | EF Core |
| P-20 | Include(Messages) on conversation list pagination | Messaging | MEDIUM | EF Core |
| P-21 | In-memory aggregation of invoices for e-reporting | Billing | MEDIUM | EF Core |
| P-22 | Sequential migration of 7 contexts at startup | Api | MEDIUM | Startup |
| P-25 | No DbContext pooling (justified by multi-tenancy) | Api | LOW | Memory |
| P-27 | Large object graph in GetPatientContextAsync | MedicalRecords | MEDIUM | Memory |

### Priority Recommendations (by impact)

1. **P-04 + P-07 + P-05**: Add missing database indexes -- cheapest fix, highest ROI.
2. **P-09**: Add pagination to ListInvoicesHandler -- prevents OOM and slow responses.
3. **P-18 + P-19**: Add `.AsSplitQuery()` or restructure multi-Include queries to avoid Cartesian explosion.
4. **P-17**: Parallelize dashboard queries with `Task.WhenAll`.
5. **P-20**: Replace `Include(Messages)` with a subquery for last message or message count.
6. **P-15**: Add output cache on ConsultationTypes endpoint.
