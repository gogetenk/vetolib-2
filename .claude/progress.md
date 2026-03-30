# progress.md -- Vetolib

_Mis a jour par l'orchestrator a chaque cycle._

## 2026-03-31T03:00 -- Extended forge session (52 PRs merged, deep quality)

- PRs merged: 52 (#157-#221)
- Unit tests: ~1309 (was 822 at start)
- develop CI: in_progress (rapid merges)

### Quality improvements
- 12 deep module audits (Agenda, Auth, Billing, MedicalRecords, Mobile, CI pipeline, Dead wiring, Endpoint wiring, TypeScript, Accessibility, Test coverage, Module decomposition)
- CRITICAL financial bugs fixed (SubTotal×Quantity, PaidAt, weighted VAT)
- CRITICAL security fixes (RandomNumberGenerator, plaintext passwords removed from events, email global uniqueness, refresh token race condition, deactivated user refresh blocked)
- CRITICAL dead wiring fixed (Notifications endpoints, AppointmentReminderService, OutputCache, MassTransit consumer discovery, audit interceptor on all DbContexts)
- Prescription stock integration wired (PrescriptionCreatedEvent now published)
- Dosage validation enforced at creation (not just preflight)
- MustChangePassword enforcement in login flow
- N+1 queries fixed (SuggestSlot batch, dashboard Task.WhenAll)
- Memory bomb fixed (PatientAlertDataReader batched)
- 140 RTL directional classes fixed
- Mobile responsive (PedigreeTree, iOS zoom, touch targets)
- 3 blog articles (breeding UAE, falcon health, vet software guide)
- Onboarding wizard + legal pages (agents in progress)

### Process improvements
- Evaluator agent (DOD-based gate before merge)
- Boy scout rule (agents report anomalies)
- guard-wip-features.sh hook (blocks disabled tests)
- Anti-stagnation rule (audits must create tasks)
- Wiring audit + module decomposition audit in 12 sources
- Forge OSS repo updated with all lessons learned

## 2026-03-30T01:45 -- Nightly forge session (19 DONE, 10 PRs merged)

- TODO: 1 (API docs) | WIP: 0 | DONE: 19
- develop CI: GREEN (messaging fix merged)
- Worktrees: pruned 20 stale, 1 active (landing UAE copy)

### PRs merged this session (10)
| PR | Description |
|---|---|
| #157 | Sex enum + field |
| #158 | Falcon + Reptile species |
| #159 | MicrochipNumber field |
| #160 | Breeding scaffold |
| #161 | Frontend patient extended |
| #162 | Weight history |
| #163 | Frontend weight chart |
| #164 | Litter entity |
| #165 | Pregnancy tracking |
| #166 | HeatCycle tracking |
| #167 | Lineage + pedigree |
| #168 | Perf critical fixes |
| #169 | 36 validator tests (1139 TU) |
| #170 | Frontend breeding dashboard |
| #171 | Security CRITICAL + HIGH fixes |
| #172 | SEO CRITICAL fixes |
| #173 | Breeding step definitions |
| #174 | UX critical (brand, i18n, French) |
| #175 | Messaging Spam/Escalate enums |

### Audits produced (7)
- UX audit (31 issues: 3 CRITICAL, 10 HIGH)
- Security audit (1 CRITICAL, 5 HIGH, 7 MEDIUM)
- Performance audit (4 HIGH, 8 MEDIUM)
- Test coverage audit (validators 16%, messaging 0 TI)
- Landing/SEO/onboarding audit (3 CRITICAL)
- Notifications audit (6 dead templates, 4 stub consumers)
- Monitoring prod-readiness (4.7/10 score)

### Studies produced (3)
- API documentation study (120 endpoints, zero docs)
- Deployment readiness (4.7/10, ~1 week to prod)
- Monitoring prod-readiness (health checks disabled in prod)

### Breeders backlog: COMPLETE (12/12)
All 12 tasks from the breeders feature set are done and merged.

## 2026-03-28T17:00 -- Forge cycle (WAVE 1 DISPATCHED)

- TODO: 9 | WIP: 3 | DONE: 0 (vague breeders)
- develop CI: GREEN (run 23689313972)
- Agents actifs: 3 (Wave 1 — worktrees isolés)
  - 001: Sex enum + field (feat/back-patient-sex-001)
  - 002: MicrochipNumber field (feat/back-patient-microchip-002)
  - 003: Falcon + Reptile species (feat/back-patient-species-003)
- PRs en review: 0
- Questions PO: 13 ouvertes
- Prochaine action: surveiller agents Wave 1 → review PRs → merge → dispatch Wave 2

### Session highlights
- New PC setup: scoop, gh CLI, Aspire CLI, dotnet-ef, Jwt:Key
- 7 fix commits to unblock develop CI (migrations, pooling, outbox, orphaned files)
- PR #156 merged (MSWProvider hydration fix)
- Forge improvements: circuit breaker 3 tentatives, agent status protocol, question re-grounding
- 4 repos analyzed (superpowers, gstack, frontend-design, Claw3D) — 3 ideas adopted
- Agent Teams (experimental) identified as next evolution for forge orchestration

### Wave 1 dispatch graph
```
WAVE 1 (NOW):  001 + 002 + 003  (no dependencies)
WAVE 2:        004 + 005 + 007  (depend on Wave 1)
WAVE 3:        006 + 008 + 010 + 011
WAVE 4:        009
WAVE 5:        012
```

## 2026-03-25 -- Architecture validation: Breeders features (12 tasks created)

- TODO: 12 (breeders) + 2 (post-MVP)
- WIP: 0
- DONE: 230 files
- PRs en review: 1 (#96 draft PageContainer)
- Questions PO: 12 ouvertes
- develop CI: GREEN localement (822 TU + 127 BDD + 29 TI)

### Architecture validation

**Status: OK -- no blocking issues**

Validated against archi-spec.md and CLAUDE.md rules:
- Ardalis modular monolith (2 assemblies per module): CONFORME
- Ardalis.Result partout: CONFORME (all factories return Result<T>)
- Multi-tenancy (ClinicId global query filter): CONFORME (all entities implement IMultiTenant)
- No Shared/ modification: CONFORME (no changes needed to Shared/)
- Phase 1 in MedicalRecords: CONFORME
- Phase 2 in new Breeding module: CONFORME
- Inter-module communication via Contracts only: CONFORME (IPatientReader already exists)
- Minimal APIs only: CONFORME
- BDD-first: CONFORME (53 scenarios in 6 .feature files)

### Architecture notes

1. **PatientDto needs extension**: current PatientDto lacks Sex and MicrochipNumber fields. Task 001 adds these.
2. **IPatientReader extension**: needs `GetPatientByIdAsync()` method for Breeding module validation. Included in scaffold task 007.
3. **MODIF_GELE required**: Breeding scaffold (task 007) modifies frozen `Vetolib.Api/Program.cs` -- flagged with MODIF_SHARED authorization.
4. **No FK cross-module**: Breeding entities reference PatientId as Guid, no FK constraint to MedicalRecords tables (correct per modular monolith pattern).
5. **Offspring creation**: AddOffspring creates a Patient in MedicalRecords. Needs IPatientCreator interface in MedicalRecords.Contracts or MediatR command forwarding.

### Tasks created

| # | Task | Module | Priority | Dependencies |
|---|---|---|---|---|
| 001 | Sex enum + field | MedicalRecords | Critique | aucune |
| 002 | MicrochipNumber field | MedicalRecords | Critique | aucune |
| 003 | Species enum expansion | MedicalRecords | Haute | aucune |
| 004 | Weight history | MedicalRecords | Haute | 001, 002, 003 |
| 005 | Frontend patient extended | Frontend | Haute | 001, 002, 003 |
| 006 | Frontend weight chart | Frontend | Haute | 004 |
| 007 | Breeding module scaffold | Breeding | Critique | 001 |
| 008 | Litter entity + endpoints | Breeding | Haute | 007 |
| 009 | Lineage + pedigree | Breeding | Haute | 007, 008 |
| 010 | Pregnancy tracking | Breeding | Haute | 007 |
| 011 | HeatCycle tracking | Breeding | Moyenne | 007 |
| 012 | Frontend breeding dashboard | Frontend | Moyenne | 008, 009, 010, 011 |

### Dispatch graph (recommended parallelism)

```
WAVE 1 (parallel):  001 + 002 + 003
                        |
WAVE 2 (parallel):  004 + 005 + 007
                        |
WAVE 3 (parallel):  006 + 008 + 010 + 011
                        |
WAVE 4:             009
                        |
WAVE 5:             012
```

### Gherkin coverage

| Feature file | Scenarios | Status |
|---|---|---|
| PatientExtendedFields.feature | 11 | @wip, ready |
| WeightHistory.feature | 8 | @wip, ready |
| Litter.feature | 9 | @wip, ready |
| Lineage.feature | 6 | @wip, ready |
| Pregnancy.feature | 11 | @wip, ready |
| HeatCycle.feature | 8 | @wip, ready |
| **Total** | **53** | |

---

## 2026-03-23 -- Forge cycle (BACKLOG CLEARED -- 13 PRs merged)

- TODO: 0
- WIP: 0
- DONE: 230 files
- PRs en review: 1 (#96 draft PageContainer)
- Questions PO: 12 ouvertes
- develop CI: GREEN localement (822 TU + 127 BDD + 29 TI)
- **BACKLOG ENTIEREMENT VIDE**

### PRs mergees cette session (13 total)
| PR | Tache | Description |
|---|---|---|
| #139 | fix-bdd-tests-ci | Fix 24 BDD tests (scope, deserialization, i18n) |
| #140 | back-billing-multi-currency-002 | Devise configurable (CurrencyCode) |
| #141 | back-billing-invoice-fields-003 | Champs e-invoicing EN16931 (11 champs) |
| #142 | back-billing-multi-tax-001 | TVA multi-pays (TaxCategory, ICountryTaxResolver) |
| #143 | refacto-outbox-003 | MassTransit outbox (3 modules) |
| #144 | back-predictive-health-001 | HealthAlert domain + 10 rules + 76 TU |
| #145 | front-predictive-health-001 | Health Alerts dashboard MSW + 7 Playwright |
| #146 | back-predictive-health-002 | IPatientAlertDataReader implementation |
| #147 | back-predictive-health-003 | Health alerts API endpoints + CQRS (20 TU) |
| #148 | back-billing-facturx-gen-005 | Generateur Factur-X PDF/A-3 + XML CII |
| #149 | back-predictive-health-004 | Background job + alert generation (8 TU) |
| #150 | back-billing-einvoicing-gateway-007 | E-invoicing gateway + mock PDP (13 TU) |
| #151 | back-billing-ereporting-009 | E-reporting B2C periodique (16 TU) |

### Statistiques session
- 13 PRs mergees
- ~150 nouveaux TU (628 -> 822)
- 2 features completes : Billing e-invoicing FR + Predictive Health Alerts
- 1 refacto critique (MassTransit outbox)
- 1 fix CI (WhatsApp EncryptionKey)

## 2026-03-12 -- Forge cycle (QA calendar validation)

- TODO: 2 (post-MVP) | WIP: 0 | DONE: 193
- Agents actifs : aucun (factory idle)
- PRs en review : 0
- Questions PO : 10 ouvertes
- develop CI : GREEN
- Cleaned 6 stale todo-sonar-coverage-* duplicates (already done)

### QA Calendar Validation (browser automation)

6 screenshots captured via Playwright MCP for PO validation.

| View | Status |
|---|---|
| Week view (EN) | PASS -- colors per consultation type, today highlighted, weekends greyed |
| Day view (EN) | PASS -- detailed cards, red "now" indicator |
| Month view (EN) | PASS -- compact pills, clickable days |
| Vet filter | PASS -- 4 vets dropdown |
| RTL layout (AR) | PASS (layout mirrored) / BUG (i18n text still English) |
| Navigation | PASS -- Prev/Today/Next + Day/Week/Month toggle |

### QA Bugs Found

| Bug | Severity | Description |
|---|---|---|
| BUG-1 | MEDIUM | AR page shows English text, sidebar links hardcoded to /en/ |
| BUG-2 | LOW | Appointment click does nothing in CalendarContainer (onClick not wired) |
| BUG-3 | LOW | Empty slots not clickable (quick create not ported to CalendarContainer) |

### Prochaine action

Factory idle. 2 post-MVP tasks remaining. QA report at `src/frontend/e2e/screenshots/qa-calendar/QA-REPORT.md` ready for PO validation.

## Audit archi -- 2026-03-11 (BDD step binding audit)

- Violations critiques : 3 (taches refacto creees)
- Violations importantes : 2 (taches refacto creees)
- Conformite globale : ATTENTION

### Detail des violations

| # | Severite | Violation | Tache refacto |
|---|---|---|---|
| 1 | CRITIQUE | StockPrescriptionIntegration.feature: `Given I am logged in as a VET` has no binding (DrugInteractionSteps scoped to wrong feature) | `todo-refacto-20260311-bdd-steps-001` |
| 2 | CRITIQUE | StockPrescriptionSteps: step text `"Or skip stock decrement entirely"` does not match feature text `"I should be able to skip stock decrement entirely"` | `todo-refacto-20260311-bdd-steps-002` |
| 3 | CRITIQUE | 28 duplicate step bindings across 10 scoped files shadow SharedSteps (divergent error storage: private fields vs ScenarioContext) | `todo-refacto-20260311-bdd-steps-005` |
| 4 | IMPORTANTE | MessageTriageSteps: `[Given]` attributes on steps used as `When` (via `And`) -- should use `[StepDefinition]` for safety | `todo-refacto-20260311-bdd-steps-003` |
| 5 | IMPORTANTE | StockPrescriptionSteps: ALL 17 step methods throw `PendingStepException` -- 8 scenarios permanently failing | `todo-refacto-20260311-bdd-steps-004` |

### Duplicate bindings detail (task 005)

5 step patterns duplicated across SharedSteps (unscoped) and scoped feature steps:
- `a clinic "(.*)"` -- 5 scoped duplicates (Billing, Dashboard, ChangePassword, Audit, Agenda)
- `I am authenticated as <ROLE>` -- 6 scoped duplicates (Agenda x2, Billing, Dashboard x2, Audit x2)
- `I am logged in as a <ROLE>` -- 4 scoped duplicates (DrugInteraction x4)
- `the system rejects with code "(.*)"` -- 3 scoped duplicates (Billing, Login, Stock)
- `the error message is "(.*)"` -- 2 scoped duplicates (Billing, Login)

All duplicates are in `[Scope]`-annotated classes so no compile-time ambiguity, but the implementations diverge (private fields vs ScenarioContext storage), creating silent failure risk if scopes drift.

### Verified non-issues

- OwnerPortalSteps `GivenTheCurrentTimeIsFridayEvening` -- binding exists and matches feature text correctly
- Agenda SlotSuggestionSteps -- no Friday scenario exists in the Agenda features
- MessageTriageSteps Given/When keyword mismatch -- Reqnroll matches across keywords, but `[StepDefinition]` is safer
- `I am authenticated as a user with role "(.*)"` -- duplicated across 5 files but all scoped and pattern not in SharedSteps (no ambiguity)
- Messaging role steps (`role "Admin"`, `role "Vet"`, etc.) -- all scoped with literal strings, no overlap with SharedSteps regex

## 2026-03-10 -- Forge cycle (nightly quality sweep)

- TODO: 4 | WIP: 0 | DONE: 228
- Agents actifs : aucun (factory idle)
- PRs en review : 5
- Questions PO : 9 ouvertes
- Disputes : aucun
- Nightly sweep: 3 cycles completed (arch audit, quality scan, robustness scan) + security scan
- Security fixes: rate limiting on auth endpoints, CORS policy, portal test-token restricted, token log redacted

### Completed this session (28+ tasks)

| Task | Type | Result |
|---|---|---|
| refacto-audit-001 | CRITICAL | Notifications throw new -- comments standardized for MassTransit retry |
| refacto-audit-002 | CRITICAL | Preferences SystemDefaults -- converted to Result<T> |
| refacto-audit-004 | IMPORTANT | data-testid added to 152+ frontend buttons |
| refacto-audit-005 | IMPORTANT | patients.ts raw fetch replaced with apiClient |
| refacto-audit-006 | MINOR | IgnoreQueryFilters documented in disputes.md |
| refacto-nightly-001 | HIGH | Messaging ListConversations -- SQL-side pagination |
| refacto-nightly-002 | MEDIUM | Stock query handlers -- AsNoTracking added |
| refacto-nightly-003 | HIGH | Stock DecrementStockByDrugCatalogEntry -- validator created |
| refacto-nightly-004 | MEDIUM | AddPrescriptionHandler -- extracted 5 private methods |
| refacto-nightly-005 | MEDIUM | GetOnboardingStateHandler -- extracted LoadSnapshot + ComputeState |
| front-ux-audit-001 | AUDIT | 8 fix tasks created from UX audit |
| front-empty-error-states | UX | ErrorState component + wired into 7 components |
| front-landing-conversion | UX | Landing page copy optimized for conversion |
| front-fix-sidebar-logo | UX | Desktop sidebar logo added |
| front-fix-confirm-dialog | UX | window.confirm replaced with shadcn AlertDialog |
| front-fix-native-inputs | UX | Native select/input replaced with shadcn |
| front-fix-loading-skeletons | UX | Skeleton components replace text loading |
| front-fix-appointment-nav | UX | Back navigation moved to top with ArrowLeft |
| front-fix-error-states | UX | ErrorState with retry on team, invoice detail |
| back-integration-tests | INFRA | WebApplicationFactory + Testcontainers project |
| infra-git-cleanup | INFRA | 20 atomic commits on feature branch |
| nightly-loop-001 | ANALYSIS | 5 refacto tasks created from code scan |
| front-fix-accessibility | UX | WCAG AA gaps (aria-labels, keyboard) |

### Additional completions (audit agents)

| Task | Type | Result |
|---|---|---|
| front-fix-i18n-hardcoded-fr | CRITICAL | French strings replaced with next-intl (9 files) |
| front-design-system-001 | AUDIT | 5 textarea to shadcn, no other violations |
| refacto-consistency-001 | AUDIT | 4 validators added, 1 endpoint fixed, route versioning task created |
| audit-coverage-001 | AUDIT | 70 endpoints mapped, 5 test tasks created for gaps |

### Second wave completions

| Task | Type | Result |
|---|---|---|
| test-agenda-endpoint-gaps-001 | HIGH | GET/PUT appointment endpoints exposed + 4 Gherkin scenarios |
| test-auth-change-password-001 | HIGH | POST /change-password endpoint + 4 Gherkin scenarios |
| test-billing-pdf-001 | HIGH | 3 PDF download Gherkin scenarios |
| test-dashboard-analytics-001 | MEDIUM | 3 analytics Gherkin scenarios (200, 401, 403) |
| test-agenda-status-edge-cases-001 | MEDIUM | 3 status edge case scenarios (200, 400, 404) |
| refacto-route-versioning-001 | MEDIUM | 40+ files migrated /api/ -> /api/v1/ |

### Remaining TODOs (5)

| Task | Priority | Status |
|---|---|---|
| refacto-audit-003 | IMPORTANT | Messaging PortalClinicContext (complex arch, needs discussion) |
| front-a11y-001 | MEDIUM | Broader WCAG AA audit |
| nightly-loop-002 | LOW | Next analysis cycle |
| back-api-public-001 | POST-MVP | API publique + OpenAPI |
| back-multi-clinic-001 | POST-MVP | Multi-clinic group view |

---

## Audit archi -- 2026-03-10 (Complet)

- Violations critiques : 2 (taches refacto creees)
- Violations importantes : 3 (taches refacto creees)
- Violations mineures : 1 (tache refacto creee)
- Conformite globale : ATTENTION

### Detail des violations

| # | Severite | Violation | Tache refacto |
|---|---|---|---|
| 1 | CRITIQUE | `throw new` dans 4 consumers Notifications | `todo-refacto-20260310-audit-001` |
| 2 | CRITIQUE | `throw new` dans Preferences SystemDefaults (domain) | `todo-refacto-20260310-audit-002` |
| 3 | IMPORTANTE | IgnoreQueryFilters massif dans Messaging (9 fichiers) -- pattern architectural manquant | `todo-refacto-20260310-audit-003` |
| 4 | IMPORTANTE | 152/165 boutons frontend sans data-testid (92% non-conforme) | `todo-refacto-20260310-audit-004` |
| 5 | IMPORTANTE | patients.ts utilise fetch brut au lieu de apiClient (2 fonctions) | `todo-refacto-20260310-audit-005` |
| 6 | MINEURE | IgnoreQueryFilters dans AppointmentReminderService sans ref disputes.md | `todo-refacto-20260310-audit-006` |

### Conformites validees (PASS)

- Isolation 2-assembly : aucune reference croisee entre runtimes de modules
- Controllers : zero utilisation de ControllerBase / ApiController
- Result<T> sur handlers : tous les handlers retournent Result<T>
- fetch frontend : tous les appels sont dans lib/api/ ou app/api/ (sauf patients.ts import)
- N+1 queries : aucun pattern foreach+await detecte
- Cross-module references : uniquement via .Contracts ou .Shared (conforme)

## 2026-03-10 -- Forge cycle

- TODO: 2 (post-MVP) | WIP: 0 | DONE: 175 (126 feature + 49 refacto)
- Agents actifs : aucun
- PRs en review : 5 (dont 4 DEV_DONE, 1 MERGED)
- Questions PO : 9 ouvertes
- Disputes : aucun
- Prochaine action : les 2 TODO restantes sont post-MVP (api-public, multi-clinic) -- factory idle

## 2026-03-10 -- Forge cycle (Sentry impl)

- TODO: 2 | WIP: 0 | DONE: 175 | PRs: 1 (Sentry)
- Sentry implementation completed on `feat/infra-sentry-impl-001` -- branch pushed, PR pending user approval
- Remaining TODOs are post-MVP: `back-api-public-001`, `back-multi-clinic-001`
- Questions PO : 9 ouvertes
- Prochaine action : merge Sentry PR, then factory is feature-complete for MVP

## 2026-03-10 -- Sentry study completed

- Study `done-infra-sentry-study-001` completed by architect agent
- Recommendation: SDK Sentry.AspNetCore + OpenTelemetry bridge + Serilog sink (Option B)
- Implementation task `todo-infra-sentry-impl-001` updated with precise steps
- Full study published in `docs/sentry-setup-study.md`

## 2026-03-10 -- Cycle forge (vague 4)

- TODO: 17 | WIP: 6 agents | DONE: 114
- Agents actifs : messaging notifications, messaging integrations (x3), prescriptions override, front agenda+billing+medical, prescriptions stock link (x2)
- PRs en review : 0
- Questions PO : 4 ouvertes
- Refacto : 3 nouvelles taches (messaging DI critique, portal auth duplication, IgnoreQueryFilters doc)

## Audit archi -- 2026-03-10 (Module Messaging)

- **Critique** : IMessageRouter + ITriageOrchestrator non enregistres dans le DI -> tache refacto 008
- **Important** : Endpoint /portal/categories duplique la logique d'auth magic link -> tache refacto 009
- **Mineur** : IgnoreQueryFilters (15 occurrences) insuffisamment documente -> tache refacto 010
- Conformite globale : ATTENTION (1 critique bloquant le runtime)

## Modules completes (DONE: 114)

### Core
- Auth, Agenda, MedicalRecords, Billing, Notifications -- matures
- AI scaffold (triage, no-show, interactions) -- operationnel
- Stock management -- operationnel

### Messaging (QUASI-COMPLET)
| Task | Statut |
|---|---|
| domain + hours + staff handlers + owner portal + templates + SSE | DONE |
| notifications + escalation | WIP (agent actif) |
| agenda + medical + AI integrations | WIP (agent actif) |
| front conversation detail + admin settings + owner portal + realtime | DONE |
| front playwright E2E | TODO |

### Prescriptions AI (EN COURS)
| Task | Statut |
|---|---|
| catalog + enrichment + weight + interactions | DONE |
| override + audit trail | WIP (agent actif) |
| stock link + stock suggest | WIP (agent actif) |
| front catalog + alerts | DONE |
| front stock + E2E | TODO |

### Infrastructure
| Task | Statut |
|---|---|
| Docker, HTTPS, monitoring, secrets, audit, rate limiting | DONE |
| CI pipeline (quality gates, security scan) | DONE |
| CD pipeline (staging + prod mock) | DONE |
| SemVer release-please | DONE |
| Sentry study | DONE |
| Sentry implementation | DONE (PR pending) |

### Taches restantes (TODO: 2 -- post-MVP)
- back-api-public-001 (API publique + OpenAPI + Webhooks)
- back-multi-clinic-001 (multi-clinic group view)
