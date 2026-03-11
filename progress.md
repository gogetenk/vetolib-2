# progress.md -- Vetolib

_Mis a jour par l'orchestrator a chaque cycle._

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
| refacto-audit-001 | CRITICAL | Notifications throw new — comments standardized for MassTransit retry |
| refacto-audit-002 | CRITICAL | Preferences SystemDefaults — converted to Result<T> |
| refacto-audit-004 | IMPORTANT | data-testid added to 152+ frontend buttons |
| refacto-audit-005 | IMPORTANT | patients.ts raw fetch replaced with apiClient |
| refacto-audit-006 | MINOR | IgnoreQueryFilters documented in disputes.md |
| refacto-nightly-001 | HIGH | Messaging ListConversations — SQL-side pagination |
| refacto-nightly-002 | MEDIUM | Stock query handlers — AsNoTracking added |
| refacto-nightly-003 | HIGH | Stock DecrementStockByDrugCatalogEntry — validator created |
| refacto-nightly-004 | MEDIUM | AddPrescriptionHandler — extracted 5 private methods |
| refacto-nightly-005 | MEDIUM | GetOnboardingStateHandler — extracted LoadSnapshot + ComputeState |
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
| refacto-route-versioning-001 | MEDIUM | 40+ files migrated /api/ → /api/v1/ |

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
