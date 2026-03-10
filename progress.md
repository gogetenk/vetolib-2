# progress.md — Vetolib

_Mis à jour par l'orchestrator à chaque cycle._

## 2026-03-10 — Forge cycle

- TODO: 2 (post-MVP) | WIP: 0 | DONE: 175 (126 feature + 49 refacto)
- Agents actifs : aucun
- PRs en review : 5 (dont 4 DEV_DONE, 1 MERGED)
- Questions PO : 9 ouvertes
- Disputes : aucun
- Prochaine action : les 2 TODO restantes sont post-MVP (api-public, multi-clinic) — factory idle

## 2026-03-10 — Forge cycle (Sentry impl)

- TODO: 2 | WIP: 0 | DONE: 175 | PRs: 1 (Sentry)
- Sentry implementation completed on `feat/infra-sentry-impl-001` — branch pushed, PR pending user approval
- Remaining TODOs are post-MVP: `back-api-public-001`, `back-multi-clinic-001`
- Questions PO : 9 ouvertes
- Prochaine action : merge Sentry PR, then factory is feature-complete for MVP

## 2026-03-10 — Sentry study completed

- Study `done-infra-sentry-study-001` completed by architect agent
- Recommendation: SDK Sentry.AspNetCore + OpenTelemetry bridge + Serilog sink (Option B)
- Implementation task `todo-infra-sentry-impl-001` updated with precise steps
- Full study published in `docs/sentry-setup-study.md`

## 2026-03-10 — Cycle forge (vague 4)

- TODO: 17 | WIP: 6 agents | DONE: 114
- Agents actifs : messaging notifications, messaging integrations (x3), prescriptions override, front agenda+billing+medical, prescriptions stock link (x2)
- PRs en review : 0
- Questions PO : 4 ouvertes
- Refacto : 3 nouvelles tâches (messaging DI critique, portal auth duplication, IgnoreQueryFilters doc)

## Audit archi — 2026-03-10 (Module Messaging)

- **Critique** : IMessageRouter + ITriageOrchestrator non enregistrés dans le DI → tâche refacto 008
- **Important** : Endpoint /portal/categories duplique la logique d'auth magic link → tâche refacto 009
- **Mineur** : IgnoreQueryFilters (15 occurrences) insuffisamment documenté → tâche refacto 010
- Conformité globale : ATTENTION (1 critique bloquant le runtime)

## Modules complétés (DONE: 114)

### Core
- Auth, Agenda, MedicalRecords, Billing, Notifications — matures
- AI scaffold (triage, no-show, interactions) — opérationnel
- Stock management — opérationnel

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

### Tâches restantes (TODO: 2 — post-MVP)
- back-api-public-001 (API publique + OpenAPI + Webhooks)
- back-multi-clinic-001 (multi-clinic group view)
